using System.Net;
using System.Net.Http.Json;
using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marketplace.Integration.Tests;

public class CartEndpointTests : IntegrationTestsBase
{
    public CartEndpointTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Get_ReturnsCurrentUserCartItems()
    {
        var (lot, cartItem) = await SeedCartItemAsync(quantity: 2);

        var response = await _client.GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CartResponse>();
        Assert.NotNull(payload);
        var item = Assert.Single(payload!.Items);
        Assert.Equal(cartItem.Quantity, item.Quantity);
        Assert.Equal(TestAuthHandler.UserId, item.UserId);
        Assert.Equal(lot.Id, item.Lot.Id);
        Assert.Equal(lot.Name, item.Lot.Name);
    }

    [Fact]
    public async Task Post_AddsItemToCart()
    {
        var lot = await SeedLotAsync();

        var response = await _client.PostAsJsonAsync("/api/cart", new AddCartItemRequest
        {
            LotId = lot.Id,
            Quantity = 3
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var storedItem = await db.CartItems.SingleAsync(c => c.UserId == TestAuthHandler.UserId && c.LotId == lot.Id);
        Assert.Equal(3, storedItem.Quantity);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenQuantityInvalid()
    {
        var lot = await SeedLotAsync();

        var response = await _client.PostAsJsonAsync("/api/cart", new AddCartItemRequest
        {
            LotId = lot.Id,
            Quantity = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        Assert.False(await db.CartItems.AnyAsync());
    }

    [Fact]
    public async Task Put_UpdatesCartItemQuantity()
    {
        var (lot, _) = await SeedCartItemAsync(quantity: 1);

        var response = await _client.PutAsJsonAsync($"/api/cart/lot/{lot.Id}", new ChangeCartItemQuantityRequest
        {
            Quantity = 5
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var updated = await db.CartItems.SingleAsync(c => c.LotId == lot.Id && c.UserId == TestAuthHandler.UserId);
        Assert.Equal(5, updated.Quantity);
    }

    [Fact]
    public async Task Put_ReturnsNotFound_WhenCartItemMissing()
    {
        var response = await _client.PutAsJsonAsync($"/api/cart/lot/{Guid.NewGuid()}", new ChangeCartItemQuantityRequest
        {
            Quantity = 2
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesCartItem()
    {
        var (lot, _) = await SeedCartItemAsync(quantity: 1);

        var response = await _client.DeleteAsync($"/api/cart/lot/{lot.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        Assert.False(await db.CartItems.AnyAsync(c => c.LotId == lot.Id && c.UserId == TestAuthHandler.UserId));
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCartItemMissing()
    {
        var response = await _client.DeleteAsync($"/api/cart/lot/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<LotEntity> SeedLotAsync(Action<LotEntity>? configure = null) =>
        IntegrationTestsHelper.SeedLotAsync(
            _factory.Services,
            stockCount: 10,
            type: LotType.Simple,
            stage: LotStage.PendingApproval,
            configure: configure);

    private async Task<(LotEntity Lot, CartItemEntity CartItem)> SeedCartItemAsync(int quantity)
    {
        var lot = await SeedLotAsync();
        var cartItem = await IntegrationTestsHelper.SeedCartItemAsync(
            _factory.Services,
            lot.Id,
            quantity,
            TestAuthHandler.UserId);

        return (lot, cartItem);
    }

    private sealed class CartResponse
    {
        public List<CartItemResponse> Items { get; set; } = new();
    }

    private sealed class CartItemResponse
    {
        public LotResponse Lot { get; set; } = default!;
        public int Quantity { get; set; }
        public Guid UserId { get; set; }
    }

    private sealed class LotResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}