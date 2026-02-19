using Marketplace.Repository.MSSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Xunit;

namespace Marketplace.Integration.Tests
{
    public abstract class IntegrationTestsBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        protected const string DefaultPassword = "P@ssw0rd1!";

        protected readonly CustomWebApplicationFactory _factory;
        protected HttpClient _client = default!;
        protected ApplicationUser TestUser { get; private set; } = default!;

        public IntegrationTestsBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.Scheme);
            await EnsureTestUserExistsAsync();
        }
        public Task DisposeAsync() => Task.CompletedTask;

        private async Task EnsureTestUserExistsAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

            if (await db.Users.AnyAsync(u => u.Id == TestAuthHandler.UserId))
            {
                return;
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = new ApplicationUser
            {
                Id = TestAuthHandler.UserId,
                UserName = "integration@test.com",
                NormalizedUserName = "INTEGRATION@TEST.COM",
                Email = "integration@test.com",
                NormalizedEmail = "INTEGRATION@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                PhoneNumber = "+15555550123",
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                Password = "integration-test"
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create test user: {errors}");
            }

            TestUser = user;
        }
    }
}
