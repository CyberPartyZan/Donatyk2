using System.Net;
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
    public async Task Post_CreatesSeller_ForAuthenticatedUser_WithAvatarKey()
    {
        var key = Guid.NewGuid().ToString("N");

        var request = new SellerDto
        {
            Name = "New Seller",
            Description = "Freshly registered seller.",
            Email = "new.seller@example.com",
            PhoneNumber = "+15555550123",
            Key = key
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
        Assert.Equal("sellers/avatars", stored.Avatar!.FilePath);
        Assert.Equal(request.Key, stored.Avatar.Key);
        Assert.Equal($"{request.Key}.img", stored.Avatar.FileName);
    }

    [Fact]
    public async Task Put_UpdatesSeller_WithAvatarKey()
    {
        var seller = await SeedSellerAsync();
        var key = Guid.NewGuid().ToString("N");

        var update = new SellerDto
        {
            Id = seller.Id,
            Name = "Updated Seller",
            Description = "Updated description",
            Email = "updated@example.com",
            PhoneNumber = "+15555550999",
            Key = key
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
        Assert.Equal("sellers/avatars", updated.Avatar!.FilePath);
        Assert.Equal(update.Key, updated.Avatar.Key);
        Assert.Equal($"{update.Key}.img", updated.Avatar.FileName);
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
    public async Task GetAvatar_ReturnsImageStream_WhenBlobExists()
    {
        using var uploadContent = new MultipartFormDataContent();
        var bytes = new byte[] { 11, 22, 33, 44 };
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        uploadContent.Add(fileContent, "file", "avatar.png");

        var uploadResponse = await _client.PostAsync("/api/sellers/avatar", uploadContent);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<BlobDto>();
        Assert.NotNull(uploaded);
        Assert.False(string.IsNullOrWhiteSpace(uploaded!.Key));

        var response = await _client.GetAsync($"/api/seller/avatar/{uploaded.Key}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("image/", response.Content.Headers.ContentType?.MediaType, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public async Task GetByUserId_ReturnsSeller_WhenExists()
    {
        var seller = await SeedSellerAsync(s =>
        {
            s.UserId = TestAuthHandler.UserId;
            s.Email = "seller.by.user@example.com";
        });

        var response = await _client.GetAsync($"/api/sellers/by-user/{TestAuthHandler.UserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<SellerDto>();
        Assert.NotNull(payload);
        Assert.Equal(seller.Id, payload!.Id);
    }

    [Fact]
    public async Task GetByUserId_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.GetAsync($"/api/sellers/by-user/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<SellerEntity> SeedSellerAsync(Action<SellerEntity>? configure = null) =>
        IntegrationTestsHelper.SeedSellerAsync(_factory.Services, configure);
}