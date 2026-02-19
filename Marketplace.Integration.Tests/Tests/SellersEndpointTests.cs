using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marketplace.Integration.Tests;

public class SellersEndpointTests : IntegrationTestsBase
{
    public SellersEndpointTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_ReturnsSeededSeller()
    {
        var seller = await SeedSellerAsync();

        var response = await _client.GetAsync("/api/sellers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<SellerDto>>();
        Assert.Contains(payload!, s => s.Id == seller.Id && s.Name == seller.Name);
    }

    [Fact]
    public async Task Get_ReturnsSeller_WhenExists()
    {
        var seller = await SeedSellerAsync();

        var response = await _client.GetAsync($"/api/sellers/{seller.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<SellerDto>();
        Assert.Equal(seller.Id, payload!.Id);
        Assert.Equal(seller.Email, payload.Email);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenSellerMissing()
    {
        var response = await _client.GetAsync($"/api/sellers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesSeller_ForAuthenticatedUser()
    {
        var request = new SellerDto
        {
            Name = "New Seller",
            Description = "Freshly registered seller.",
            Email = "new.seller@example.com",
            PhoneNumber = "+15555550123",
            AvatarImageUrl = "https://example.com/avatar.png"
        };

        var response = await _client.PostAsJsonAsync("/api/sellers", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var stored = await db.Sellers.SingleAsync(s => s.UserId == TestAuthHandler.UserId);

        Assert.Equal(request.Name, stored.Name);
        Assert.Equal(request.Description, stored.Description);
        Assert.Equal(request.Email, stored.Email);
        Assert.Equal(request.PhoneNumber, stored.PhoneNumber);
        Assert.Equal(request.AvatarImageUrl, stored.AvatarImageUrl);
    }

    [Fact]
    public async Task Put_UpdatesSeller()
    {
        var seller = await SeedSellerAsync();

        var update = new SellerDto
        {
            Id = seller.Id,
            Name = "Updated Seller",
            Description = "Updated description",
            Email = "updated@example.com",
            PhoneNumber = "+15555550999",
            AvatarImageUrl = "https://example.com/new-avatar.png"
        };

        var response = await _client.PutAsJsonAsync($"/api/sellers/{seller.Id}", update);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var updated = await db.Sellers.SingleAsync(s => s.Id == seller.Id);

        Assert.Equal(update.Name, updated.Name);
        Assert.Equal(update.Description, updated.Description);
        Assert.Equal(update.Email, updated.Email);
        Assert.Equal(update.PhoneNumber, updated.PhoneNumber);
        Assert.Equal(update.AvatarImageUrl, updated.AvatarImageUrl);
    }

    [Fact]
    public async Task Delete_SoftDeletesSeller()
    {
        var seller = await SeedSellerAsync();

        var response = await _client.DeleteAsync($"/api/sellers/{seller.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var stored = await db.Sellers.IgnoreQueryFilters().SingleAsync(s => s.Id == seller.Id);

        Assert.True(stored.IsDeleted);
    }

    private async Task<SellerEntity> SeedSellerAsync(Action<SellerEntity>? configure = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        var sellerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"seller-{Guid.NewGuid():N}@example.com",
            NormalizedUserName = $"SELLER-{Guid.NewGuid():N}@EXAMPLE.COM",
            Email = $"seller-{Guid.NewGuid():N}@example.com",
            NormalizedEmail = $"SELLER-{Guid.NewGuid():N}@EXAMPLE.COM",
            EmailConfirmed = true,
            PasswordHash = "seller-password",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550000",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = "seller-password"
        };

        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Seed Seller",
            Description = "Seller seeded for integration tests.",
            Email = $"seed-seller-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+15555550001",
            AvatarImageUrl = "https://example.com/seed.png",
            UserId = sellerUser.Id,
            User = sellerUser,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        configure?.Invoke(seller);

        db.Users.Add(sellerUser);
        db.Sellers.Add(seller);
        await db.SaveChangesAsync();

        return seller;
    }
}