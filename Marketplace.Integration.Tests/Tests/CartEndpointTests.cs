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
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
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
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
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
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
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
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        Assert.False(await db.CartItems.AnyAsync(c => c.LotId == lot.Id && c.UserId == TestAuthHandler.UserId));
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCartItemMissing()
    {
        var response = await _client.DeleteAsync($"/api/cart/lot/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<(LotEntity Lot, CartItemEntity CartItem)> SeedCartItemAsync(int quantity)
    {
        var lot = await SeedLotAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        var entity = new CartItemEntity
        {
            LotId = lot.Id,
            Quantity = quantity,
            UserId = TestAuthHandler.UserId
        };

        db.CartItems.Add(entity);
        await db.SaveChangesAsync();

        return (lot, entity);
    }

    private async Task<LotEntity> SeedLotAsync(Action<LotEntity>? configure = null)
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

        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Category {Guid.NewGuid():N}",
            Description = "Test category for cart scenarios."
        };

        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Cart Seller",
            Description = "Seller used for cart integration tests.",
            Email = $"cart-seller-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+15555550001",
            AvatarImageUrl = string.Empty,
            UserId = sellerUser.Id,
            User = sellerUser,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var lot = new LotEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Cart Lot {Guid.NewGuid():N}".Substring(0, 16),
            Description = "Lot used for cart integration tests.",
            Price = new Money(100, Currency.USD),
            Compensation = new Money(50, Currency.USD),
            StockCount = 10,
            Discount = 0,
            Type = LotType.Simple,
            Stage = LotStage.PendingApproval,
            Seller = seller,
            Category = category,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        configure?.Invoke(lot);

        db.Users.Add(sellerUser);
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        return lot;
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