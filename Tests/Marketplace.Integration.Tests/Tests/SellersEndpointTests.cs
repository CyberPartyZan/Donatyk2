using System.Net;
using System.Net.Http.Json;
using Marketplace.Repository.MSSql;
using Microsoft.AspNetCore.Http;
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
    public async Task Post_CreatesSeller_ForAuthenticatedUser_WithAvatarBlob()
    {
        var request = new SellerDto
        {
            Name = "New Seller",
            Description = "Freshly registered seller.",
            Email = "new.seller@example.com",
            PhoneNumber = "+15555550123",
            Avatar = new BlobDto
            {
                Id = Guid.NewGuid(),
                FilePath = "sellers/avatars",
                Key = Guid.NewGuid().ToString("N"),
                FileName = "avatar.png"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/sellers", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var stored = await db.Sellers.Include(s => s.Avatar).SingleAsync(s => s.UserId == TestAuthHandler.UserId);

        Assert.Equal(request.Name, stored.Name);
        Assert.Equal(request.Description, stored.Description);
        Assert.Equal(request.Email, stored.Email);
        Assert.Equal(request.PhoneNumber, stored.PhoneNumber);
        Assert.NotNull(stored.Avatar);
        Assert.Equal(request.Avatar!.FilePath, stored.Avatar!.FilePath);
        Assert.Equal(request.Avatar.Key, stored.Avatar.Key);
        Assert.Equal(request.Avatar.FileName, stored.Avatar.FileName);
    }

    [Fact]
    public async Task Put_UpdatesSeller_WithAvatarBlob()
    {
        var seller = await SeedSellerAsync();

        var update = new SellerDto
        {
            Id = seller.Id,
            Name = "Updated Seller",
            Description = "Updated description",
            Email = "updated@example.com",
            PhoneNumber = "+15555550999",
            Avatar = new BlobDto
            {
                Id = Guid.NewGuid(),
                FilePath = "sellers/avatars",
                Key = Guid.NewGuid().ToString("N"),
                FileName = "new-avatar.png"
            }
        };

        var response = await _client.PutAsJsonAsync($"/api/sellers/{seller.Id}", update);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var updated = await db.Sellers.Include(s => s.Avatar).SingleAsync(s => s.Id == seller.Id);

        Assert.Equal(update.Name, updated.Name);
        Assert.Equal(update.Description, updated.Description);
        Assert.Equal(update.Email, updated.Email);
        Assert.Equal(update.PhoneNumber, updated.PhoneNumber);
        Assert.NotNull(updated.Avatar);
        Assert.Equal(update.Avatar!.FilePath, updated.Avatar!.FilePath);
        Assert.Equal(update.Avatar.Key, updated.Avatar.Key);
        Assert.Equal(update.Avatar.FileName, updated.Avatar.FileName);
    }

    [Fact]
    public async Task UploadAvatar_ReturnsBlob()
    {
        using var content = new MultipartFormDataContent();
        var bytes = new byte[] { 1, 2, 3, 4 };
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "avatar.png");

        var response = await _client.PostAsync("/api/sellers/avatar", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<BlobDto>();
        Assert.NotNull(payload);
        Assert.Equal("sellers/avatars", payload!.FilePath);
        Assert.Equal("avatar.png", payload.FileName);
        Assert.False(string.IsNullOrWhiteSpace(payload.Key));
    }

    [Fact]
    public async Task Delete_SoftDeletesSeller()
    {
        var seller = await SeedSellerAsync();

        var response = await _client.DeleteAsync($"/api/sellers/{seller.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var stored = await db.Sellers.IgnoreQueryFilters().SingleAsync(s => s.Id == seller.Id);

        Assert.True(stored.IsDeleted);
    }

    private Task<SellerEntity> SeedSellerAsync(Action<SellerEntity>? configure = null) =>
        IntegrationTestsHelper.SeedSellerAsync(_factory.Services, configure);
}