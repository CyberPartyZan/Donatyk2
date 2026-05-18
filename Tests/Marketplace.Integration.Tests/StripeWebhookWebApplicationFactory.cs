using Marketplace.Repository.MSSql;
using Marketplace.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Stripe;
using System.Data.Common;
using Testcontainers.MsSql;
using WireMock.Server;
using Xunit;

namespace Marketplace.Integration.Tests;

/// <summary>
/// Custom factory that activates the Stripe gateway and wires StripeConfiguration
/// to a WireMock server instead of the real Stripe API.
/// </summary>
public class StripeWebhookWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestWebhookSecret = "whsec_test_integration_secret_32ch!";

    private readonly MsSqlContainer _sqlContainer =
        new MsSqlBuilder()
            .WithPassword("yourStrong(!)Password")
            .Build();

    private DbConnection _connection = default!;
    private Respawner _respawner = default!;

    public WireMockServer WireMockServer { get; } =
        WireMockServer.Start();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        _connection = new SqlConnection(_sqlContainer.GetConnectionString());
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await db.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"]
        });
    }

    public async Task DisposeAsync()
    {
        WireMockServer.Stop();
        await _sqlContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override configuration so the app uses Stripe gateway pointed at WireMock
        builder.UseSetting("Payments:Provider", "Stripe");
        builder.UseSetting("Stripe:SecretKey", "sk_test_fakesecretkey");
        builder.UseSetting("Stripe:PublishableKey", "pk_test_fakepublishablekey");
        builder.UseSetting("Stripe:WebhookSecret", TestWebhookSecret);

        builder.ConfigureServices(services =>
        {
            // Replace SQL connection with test container
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MarketplaceDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<MarketplaceDbContext>(options =>
                options.UseSqlServer(_sqlContainer.GetConnectionString()));

            services.AddAuthentication(TestAuthHandler.Scheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.Scheme,
                        _ => { });

            // Point Stripe.net HttpClient to WireMock by replacing IStripeClient in DI
            var stripeDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IStripeClient));
            if (stripeDescriptor != null)
                services.Remove(stripeDescriptor);

            services.AddSingleton<IStripeClient>(_ =>
                new Stripe.StripeClient(
                    apiKey: "sk_test_fakesecretkey",
                    httpClient: null,
                    apiBase: WireMockServer.Urls[0]
                ));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async Task EnsureTestUserAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        if (await db.Users.AnyAsync(u => u.Id == TestAuthHandler.UserId))
            return;

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Id = TestAuthHandler.UserId,
            UserName = "stripe-integration@test.com",
            NormalizedUserName = "STRIPE-INTEGRATION@TEST.COM",
            Email = "stripe-integration@test.com",
            NormalizedEmail = "STRIPE-INTEGRATION@TEST.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550199",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = "stripe-integration-test"
        };

        var result = await userManager.CreateAsync(user, "P@ssw0rd1!");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create Stripe test user: {errors}");
        }
    }
}