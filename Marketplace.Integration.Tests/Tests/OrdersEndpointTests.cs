using Marketplace.Repository.MSSql;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Marketplace.Integration.Tests;

public class OrdersEndpointTests : IntegrationTestsBase
{
    public OrdersEndpointTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Checkout_ReturnsRedirectAndPersistsOrder()
    {
        const int quantity = 2;
        var (lot, _, initialStock) = await SeedCartItemAsync(quantity);

        var request = CreateCheckoutRequest();

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("payments", response.Headers.Location!.Host, StringComparison.OrdinalIgnoreCase);

        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders.Include(o => o.Items).SingleAsync(o => o.Id == orderId);
        Assert.Equal(TestAuthHandler.UserId, order.CustomerId);
        Assert.Single(order.Items);
        var orderItem = order.Items.Single();
        Assert.Equal(lot.Id, orderItem.LotId);
        Assert.Equal(quantity, orderItem.Quantity);
        Assert.Equal(OrderStatus.Created, order.Status);

        var lotAfter = await db.Lots.SingleAsync(l => l.Id == lot.Id);
        Assert.Equal(initialStock - quantity, lotAfter.StockCount);

        Assert.False(await db.CartItems.AnyAsync(c => c.UserId == TestAuthHandler.UserId));
    }

    [Fact]
    public async Task PaymentWebhook_MarksOrderPaid_WhenSuccessful()
    {
        const string provider = "FakePay";
        const string reference = "PAY-123456";
        await SeedCartItemAsync(1);

        var checkoutResponse = await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest(provider));
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        var webhookResponse = await _client.PostAsJsonAsync("/api/orders/payment/webhook", new PaymentWebhookRequest
        {
            OrderId = orderId,
            Provider = provider,
            Reference = reference,
            IsSuccess = true
        });

        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var order = await db.Orders.SingleAsync(o => o.Id == orderId);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(reference, order.PaymentReference);
    }

    private CheckoutRequest CreateCheckoutRequest(string provider = "FakePay", decimal taxRate = 0.07m)
    {
        return new CheckoutRequest
        {
            Shipping = new ShippingInfoDto
            {
                RecipientName = "Test Buyer",
                Line1 = "123 Test Street",
                City = "Testville",
                State = "TS",
                PostalCode = "12345",
                Country = "US",
                Phone = "+15555551234"
            },
            Payment = new PaymentInfoDto
            {
                Provider = provider,
                TaxRate = taxRate,
                ReturnUrl = "https://example.com/return"
            }
        };
    }

    private async Task<(LotEntity Lot, CartItemEntity CartItem, int StockBefore)> SeedCartItemAsync(int quantity)
    {
        var lot = await SeedLotAsync(stockCount: 10);
        var stockBefore = lot.StockCount;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var cartItem = new CartItemEntity
        {
            LotId = lot.Id,
            Quantity = quantity,
            UserId = TestAuthHandler.UserId
        };

        db.CartItems.Add(cartItem);
        await db.SaveChangesAsync();

        return (lot, cartItem, stockBefore);
    }

    private async Task<LotEntity> SeedLotAsync(int stockCount = 10)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

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
            Name = $"Orders Category {Guid.NewGuid():N}",
            Description = "Orders integration category."
        };

        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Orders Seller",
            Description = "Seller for orders integration tests.",
            Email = $"orders-seller-{Guid.NewGuid():N}@example.com",
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
            Name = $"Order Lot {Guid.NewGuid():N}".Substring(0, 16),
            Description = "Lot seeded for orders integration tests.",
            Price = new Money(100, Currency.USD),
            Compensation = new Money(60, Currency.USD),
            StockCount = stockCount,
            DiscountedPrice = null,
            Type = LotType.Simple,
            Stage = LotStage.Approved,
            Seller = seller,
            Category = category,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        db.Users.Add(sellerUser);
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        return lot;
    }
}