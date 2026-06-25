using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Marketplace.Integration.Tests;

public class UsersEndpointTests : IntegrationTestsBase
{
    public UsersEndpointTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

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
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var updated = await db.Users.SingleAsync(u => u.Id == seeded.Id);

        Assert.Equal(dto.Email, updated.Email);
        Assert.Equal(dto.Email.ToUpperInvariant(), updated.NormalizedEmail);
        Assert.Equal(dto.EmailConfirmed, updated.EmailConfirmed);
        Assert.Equal(dto.LockoutEnabled, updated.LockoutEnabled);
        Assert.Equal(dto.LockoutEnd, updated.LockoutEnd);
    }

    [Fact]
    public async Task GetAll_SetsTotalCountHeader_ForSameFilter()
    {
        var marker = $"users-count-{Guid.NewGuid():N}";

        await SeedUserAsync(user =>
        {
            var email = $"{marker}@example.com";
            user.Email = email;
            user.NormalizedEmail = email.ToUpperInvariant();
            user.UserName = email;
            user.NormalizedUserName = email.ToUpperInvariant();
        });

        await SeedUserAsync();

        var response = await _client.GetAsync($"/api/users?search={Uri.EscapeDataString(marker)}&page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Total-Count", out var values));

        var totalCount = int.Parse(values!.Single());
        Assert.Equal(1, totalCount);
    }

    private Task<ApplicationUser> SeedUserAsync(Action<ApplicationUser>? configure = null) =>
        IntegrationTestsHelper.SeedUserAsync(_factory.Services, configure);
}