using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Enums;
using Donatyk2.Server.ValueObjects;
using Marketplace.Integration.Tests.Authentication;
using Marketplace.Repository.MSSql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

public class UsersEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public UsersEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.Scheme);
        await EnsureAdminUserExistsAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ReturnsSeededUser()
    {
        var seeded = await SeedUserAsync();

        var response = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.Contains(payload!, user => user.Id == seeded.Id && user.Email == seeded.Email);
    }

    [Fact]
    public async Task Get_ReturnsUser_WhenAdminRequests()
    {
        var seeded = await SeedUserAsync();

        var response = await _client.GetAsync($"/api/users/{seeded.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(seeded.Id, payload!.Id);
        Assert.Equal(seeded.Email, payload.Email);
    }

    [Fact]
    public async Task GetByEmail_ReturnsUser()
    {
        var seeded = await SeedUserAsync();

        var response = await _client.GetAsync($"/api/users/by-email?email={Uri.EscapeDataString(seeded.Email!)}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(seeded.Id, payload!.Id);
        Assert.Equal(seeded.Email, payload.Email);
    }

    [Fact]
    public async Task Put_UpdatesUserDetails()
    {
        var seeded = await SeedUserAsync(user =>
        {
            user.EmailConfirmed = false;
            user.LockoutEnabled = false;
        });

        var dto = new UserDto
        {
            Email = "updated.user@example.com",
            EmailConfirmed = true,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddDays(1)
        };

        var response = await _client.PutAsJsonAsync($"/api/users/{seeded.Id}", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var updated = await db.Users.SingleAsync(u => u.Id == seeded.Id);

        Assert.Equal(dto.Email, updated.Email);
        Assert.Equal(dto.Email.ToUpperInvariant(), updated.NormalizedEmail);
        Assert.Equal(dto.EmailConfirmed, updated.EmailConfirmed);
        Assert.Equal(dto.LockoutEnabled, updated.LockoutEnabled);
        Assert.Equal(dto.LockoutEnd, updated.LockoutEnd);
    }

    private async Task EnsureAdminUserExistsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        if (await db.Users.AnyAsync(u => u.Id == TestAuthHandler.UserId))
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = TestAuthHandler.UserId,
            UserName = "integration@test.com",
            NormalizedUserName = "INTEGRATION@TEST.COM",
            Email = "integration@test.com",
            NormalizedEmail = "INTEGRATION@TEST.COM",
            EmailConfirmed = true,
            PasswordHash = "integration-test",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550123",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = "integration-test"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private async Task<ApplicationUser> SeedUserAsync(Action<ApplicationUser>? configure = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        var unique = Guid.NewGuid().ToString("N");
        var email = $"user-{unique}@example.com";

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = "integration-user",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550000",
            PhoneNumberConfirmed = true,
            LockoutEnabled = false,
            CreatedAt = DateTime.UtcNow,
            Password = "integration-user"
        };

        configure?.Invoke(user);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }
}