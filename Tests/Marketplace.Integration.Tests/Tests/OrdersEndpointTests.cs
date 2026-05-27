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

        var checkoutResponse = await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest("Stripe"));
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        var webhookResponse = await _client.PostAsJsonAsync("/api/orders/payment/webhook", new PaymentWebhookRequest
        {
            OrderId = orderId,
            Provider = "Stripe",
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
    public async Task DrawPaymentWebhook_WhenPaymentSucceeded_MarksOrderAndTicketsPaid()
    {
        const int ticketsCount = 2;
        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Draw);

        var checkoutResponse = await _client.PostAsJsonAsync(
            "/api/orders/checkout/draw",
            CreateCheckoutDrawRequest(lot.Id, ticketsCount, provider: "Stripe"));

        Assert.Equal(HttpStatusCode.Redirect, checkoutResponse.StatusCode);
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        var webhookResponse = await _client.PostAsJsonAsync(
            $"/api/orders/payment/draw/webhook?lotId={lot.Id}",
            new DrawPaymentWebhookRequest
            {
                OrderId = orderId,
                LotId = lot.Id,
                Provider = "Stripe",
                Reference = "DRAW-PAY-001",
                IsSuccess = true
            });

        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders.SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Paid, order.Status);

        var paidTickets = await db.Tickets
            .Where(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId)
            .ToListAsync();

        Assert.Equal(ticketsCount, paidTickets.Count);
        Assert.All(paidTickets, t => Assert.True(t.IsPayed));
    }

    [Fact]
    public async Task DrawPaymentWebhook_WhenPaymentFailed_CancelsTicketsAndOrder()
    {
        const int ticketsCount = 3;
        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Draw);

        var checkoutResponse = await _client.PostAsJsonAsync(
            "/api/orders/checkout/draw",
            CreateCheckoutDrawRequest(lot.Id, ticketsCount, provider: "Stripe"));

        Assert.Equal(HttpStatusCode.Redirect, checkoutResponse.StatusCode);
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
            var ticketsBefore = await db.Tickets.CountAsync(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId);
            Assert.Equal(ticketsCount, ticketsBefore);
        }

        var webhookResponse = await _client.PostAsJsonAsync(
            $"/api/orders/payment/draw/webhook?lotId={lot.Id}",
            new DrawPaymentWebhookRequest
            {
                OrderId = orderId,
                LotId = lot.Id,
                IsSuccess = false
            });

        Assert.Equal(HttpStatusCode.OK, webhookResponse.StatusCode);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await verifyDb.Orders.SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.Cancelled, order.Status);

        var remainingTickets = await verifyDb.Tickets
            .CountAsync(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId);
        Assert.Equal(0, remainingTickets);

        var lotAfter = await verifyDb.Lots.SingleAsync(l => l.Id == lot.Id);
        Assert.Equal(0, lotAfter.TicketsSold);
    }

    [Fact]
    public async Task PaymentWebhook_ForDrawCheckout_MarksTicketsAsPayed()
    {
        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Draw);

        var checkoutResponse = await _client.PostAsJsonAsync(
            "/api/orders/checkout/draw",
            CreateCheckoutDrawRequest(lot.Id, ticketsCount: 2, provider: "Stripe"));

        Assert.Equal(HttpStatusCode.Redirect, checkoutResponse.StatusCode);
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        var webhookResponse = await _client.PostAsJsonAsync("/api/orders/payment/webhook", new PaymentWebhookRequest
        {
            OrderId = orderId,
            Provider = "Stripe",
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

    private static CheckoutRequest CreateCheckoutRequest(string provider = "Stripe", decimal taxRate = 0.07m) =>
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
        string provider = "Stripe",
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
        string provider = "Stripe",
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

    private async Task<(LotEntity Lot, CartItemEntity CartItem, int StockBefore)> SeedCartItemAsync(
        int quantity, LotType type)
    {
        var lot = await SeedLotAsync(stockCount: 10, type: type);
        var cartItem = await IntegrationTestsHelper.SeedCartItemAsync(
            _factory.Services, lot.Id, quantity, TestAuthHandler.UserId);
        return (lot, cartItem, lot.StockCount);
    }

    private Task<LotEntity> SeedLotAsync(int stockCount, LotType type) =>
        IntegrationTestsHelper.SeedLotAsync(
            _factory.Services,
            stockCount: stockCount,
            type: type,
            stage: LotStage.Approved);

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