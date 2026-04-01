using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Marketplace.Integration.Tests;

public class OrdersEndpointTests : IntegrationTestsBase
{
    public OrdersEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Checkout_ReturnsRedirectAndPersistsOrder()
    {
        const int quantity = 2;
        var (lot, _, initialStock) = await SeedCartItemAsync(quantity, LotType.Simple);

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest());

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
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
    public async Task Checkout_WhenDrawLotInCart_ReturnsInternalServerError()
    {
        await SeedCartItemAsync(1, LotType.Draw);

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest());

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WhenAuctionLotInCart_ReturnsInternalServerError()
    {
        await SeedCartItemAsync(1, LotType.Auction);

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest());

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task PaymentWebhook_MarksOrderPaid_WhenSuccessful()
    {
        await SeedCartItemAsync(1, LotType.Simple);

        var checkoutResponse = await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest("FakePay"));
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        var webhookResponse = await _client.PostAsJsonAsync("/api/orders/payment/webhook", new PaymentWebhookRequest
        {
            OrderId = orderId,
            Provider = "FakePay",
            Reference = "PAY-123456",
            IsSuccess = true
        });

        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var order = await db.Orders.SingleAsync(o => o.Id == orderId);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal("PAY-123456", order.PaymentReference);
    }

    [Fact]
    public async Task CheckoutDraw_ReturnsRedirectAndPersistsOrderAndTickets()
    {
        const int ticketsCount = 3;
        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Draw);

        var response = await _client.PostAsJsonAsync(
            "/api/orders/checkout/draw",
            CreateCheckoutDrawRequest(lot.Id, ticketsCount));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders.Include(o => o.Items).SingleAsync(o => o.Id == orderId);
        Assert.Equal(TestAuthHandler.UserId, order.CustomerId);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Single(order.Items);

        var orderItem = order.Items.Single();
        Assert.Equal(lot.Id, orderItem.LotId);
        Assert.Equal(ticketsCount, orderItem.Quantity);

        var ticketsCountInDb = await db.Tickets.CountAsync(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId);
        Assert.Equal(ticketsCount, ticketsCountInDb);

        var lotAfter = await db.Lots.SingleAsync(l => l.Id == lot.Id);
        Assert.Equal(ticketsCount, lotAfter.TicketsSold);
    }

    [Fact]
    public async Task CheckoutAuction_ReturnsRedirectAndPersistsOrderAndBid()
    {
        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Auction);
        var request = CreateCheckoutAuctionRequest(lot.Id, new Money(125m, Currency.USD));

        var response = await _client.PostAsJsonAsync("/api/orders/checkout/auction", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders.Include(o => o.Items).SingleAsync(o => o.Id == orderId);
        Assert.Equal(TestAuthHandler.UserId, order.CustomerId);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Single(order.Items);

        var orderItem = order.Items.Single();
        Assert.Equal(lot.Id, orderItem.LotId);
        Assert.Equal(1, orderItem.Quantity);
        Assert.Equal(125m, orderItem.UnitPrice.Amount);

        var bid = await db.BidHistory.SingleAsync(b => b.AuctionId == lot.Id && b.BidderId == TestAuthHandler.UserId);
        Assert.Equal(125m, bid.Amount.Amount);
        Assert.Equal(Currency.USD, bid.Amount.Currency);

        var lotAfter = await db.Lots.SingleAsync(l => l.Id == lot.Id);
        Assert.Equal(125m, lotAfter.Price.Amount);
        Assert.Equal(Currency.USD, lotAfter.Price.Currency);
    }

    [Fact]
    public async Task PaymentWebhook_ForDrawCheckout_MarksTicketsAsPayed()
    {
        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Draw);

        var checkoutResponse = await _client.PostAsJsonAsync(
            "/api/orders/checkout/draw",
            CreateCheckoutDrawRequest(lot.Id, ticketsCount: 2, provider: "FakePay"));

        Assert.Equal(HttpStatusCode.Redirect, checkoutResponse.StatusCode);
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        var webhookResponse = await _client.PostAsJsonAsync("/api/orders/payment/webhook", new PaymentWebhookRequest
        {
            OrderId = orderId,
            Provider = "FakePay",
            Reference = "DRAW-PAY-001",
            IsSuccess = true
        });

        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        await WaitUntilAsync(async () =>
            await db.Tickets
                .Where(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId)
                .AnyAsync() &&
            await db.Tickets
                .Where(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId)
                .AllAsync(t => t.IsPayed),
            timeout: TimeSpan.FromSeconds(10),
            delay: TimeSpan.FromMilliseconds(250));

        var paidTickets = await db.Tickets
            .Where(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId)
            .ToListAsync();

        Assert.Equal(2, paidTickets.Count);
        Assert.All(paidTickets, t => Assert.True(t.IsPayed));
    }

    private static CheckoutRequest CreateCheckoutRequest(string provider = "FakePay", decimal taxRate = 0.07m) =>
        new()
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

    private static CheckoutDrawRequest CreateCheckoutDrawRequest(
        Guid lotId,
        int ticketsCount,
        string provider = "FakePay",
        decimal taxRate = 0.07m) =>
        new()
        {
            LotId = lotId,
            TicketsCount = ticketsCount,
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

    private static CheckoutAuctionRequest CreateCheckoutAuctionRequest(
        Guid lotId,
        Money amount,
        string provider = "FakePay",
        decimal taxRate = 0.07m) =>
        new()
        {
            LotId = lotId,
            Amount = amount,
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

    private async Task<(LotEntity Lot, CartItemEntity CartItem, int StockBefore)> SeedCartItemAsync(int quantity, LotType type)
    {
        var lot = await SeedLotAsync(stockCount: 10, type: type);
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

    private async Task<LotEntity> SeedLotAsync(int stockCount, LotType type)
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
            Type = type,
            Stage = LotStage.Approved,
            Seller = seller,
            Category = category,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            EndOfAuction = type == LotType.Auction ? DateTime.UtcNow.AddHours(2) : null,
            AuctionStepPercent = type == LotType.Auction ? 5 : null,
            TicketPrice = type == LotType.Draw ? new Money(5, Currency.USD) : null,
            TicketsSold = type == LotType.Draw ? 0 : null,
            IsDrawn = false
        };

        db.Users.Add(sellerUser);
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        return lot;
    }

    private static async Task WaitUntilAsync(
        Func<Task<bool>> condition,
        TimeSpan timeout,
        TimeSpan delay)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(delay);
        }

        Assert.True(await condition());
    }
}