using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Marketplace.Integration.Tests;

public class DeliveryPreferencesCheckoutTests : IntegrationTestsBase
{
    public DeliveryPreferencesCheckoutTests(CustomWebApplicationFactory factory) : base(factory) { }

    // ── Checkout (Simple lot) ────────────────────────────────────────────────

    [Fact]
    public async Task Checkout_WithDeliveryPreferenceId_UsesPreferenceAddressAndCarrier()
    {
        var preference = await IntegrationTestsHelper.SeedDeliveryPreferencesAsync(
            _factory.Services, TestAuthHandler.UserId, DeliveryCarrier.DHL);

        await SeedCartItemAsync(1, LotType.Simple);

        var request = CreateCheckoutRequest(preferenceId: preference.Id);
        var response = await _client.PostAsJsonAsync("/api/orders/checkout", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders
            .Include(o => o.ShippingAddress)
            .SingleAsync(o => o.Id == orderId);

        Assert.Equal(DeliveryCarrier.DHL, order.DeliveryCarrier);
        Assert.Equal(preference.ShippingAddress.RecipientName, order.ShippingAddress!.RecipientName);
        Assert.Equal(preference.ShippingAddress.Line1, order.ShippingAddress.Line1);
        Assert.Equal(preference.ShippingAddress.City, order.ShippingAddress.City);
    }

    [Fact]
    public async Task Checkout_WithInlineShipping_PersistsDeliveryPreferenceAndCarrier()
    {
        await SeedCartItemAsync(1, LotType.Simple);

        var request = CreateCheckoutRequest(carrier: DeliveryCarrier.FedEx);
        var response = await _client.PostAsJsonAsync("/api/orders/checkout", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders
            .Include(o => o.ShippingAddress)
            .SingleAsync(o => o.Id == orderId);

        Assert.Equal(DeliveryCarrier.FedEx, order.DeliveryCarrier);
        Assert.Equal("Test Buyer", order.ShippingAddress!.RecipientName);

        // A DeliveryPreferences record should be upserted
        var savedPreference = await db.DeliveryPreferences
            .Include(p => p.ShippingAddress)
            .SingleOrDefaultAsync(p => p.UserId == TestAuthHandler.UserId && p.Carrier == DeliveryCarrier.FedEx);

        Assert.NotNull(savedPreference);
        Assert.Equal("Test Buyer", savedPreference!.ShippingAddress.RecipientName);
    }

    [Fact]
    public async Task Checkout_WithInlineShipping_SameAddressAndCarrier_ReusesSavedDeliveryPreference()
    {
        await SeedCartItemAsync(1, LotType.Simple);

        // First checkout — creates the preference
        var request = CreateCheckoutRequest(carrier: DeliveryCarrier.UPS);
        var firstResponse = await _client.PostAsJsonAsync("/api/orders/checkout", request);
        Assert.Equal(HttpStatusCode.Redirect, firstResponse.StatusCode);

        // Read the saved DeliveryPreference from DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var savedPreference = await db.DeliveryPreferences
            .SingleAsync(p => p.UserId == TestAuthHandler.UserId && p.Carrier == DeliveryCarrier.UPS);

        // Second checkout with the saved preference id
        await SeedCartItemAsync(1, LotType.Simple);
        var secondRequest = CreateCheckoutRequest(preferenceId: savedPreference.Id);
        var secondResponse = await _client.PostAsJsonAsync("/api/orders/checkout", secondRequest);

        Assert.Equal(HttpStatusCode.Redirect, secondResponse.StatusCode);

        // Still only one preference record for this user+carrier
        var count = await db.DeliveryPreferences
            .CountAsync(p => p.UserId == TestAuthHandler.UserId && p.Carrier == DeliveryCarrier.UPS);

        Assert.Equal(1, count);
    }

    // ── CheckoutDraw ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckoutDraw_WithDeliveryPreferenceId_UsesPreferenceAddressAndCarrier()
    {
        var preference = await IntegrationTestsHelper.SeedDeliveryPreferencesAsync(
            _factory.Services, TestAuthHandler.UserId, DeliveryCarrier.USPS);

        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Draw);

        var request = CreateCheckoutDrawRequest(lot.Id, ticketsCount: 2, preferenceId: preference.Id);
        var response = await _client.PostAsJsonAsync("/api/orders/checkout/draw", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders
            .Include(o => o.ShippingAddress)
            .SingleAsync(o => o.Id == orderId);

        Assert.Equal(DeliveryCarrier.USPS, order.DeliveryCarrier);
        Assert.Equal(preference.ShippingAddress.RecipientName, order.ShippingAddress!.RecipientName);
    }

    // ── CheckoutAuction ──────────────────────────────────────────────────────

    [Fact]
    public async Task CheckoutAuction_WithDeliveryPreferenceId_UsesPreferenceAddressAndCarrier()
    {
        var preference = await IntegrationTestsHelper.SeedDeliveryPreferencesAsync(
            _factory.Services, TestAuthHandler.UserId, DeliveryCarrier.Royal_Mail);

        var lot = await SeedLotAsync(stockCount: 1, type: LotType.Auction);

        var request = CreateCheckoutAuctionRequest(lot.Id, new Money(125m, Currency.USD), preferenceId: preference.Id);
        var response = await _client.PostAsJsonAsync("/api/orders/checkout/auction", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders
            .Include(o => o.ShippingAddress)
            .SingleAsync(o => o.Id == orderId);

        Assert.Equal(DeliveryCarrier.Royal_Mail, order.DeliveryCarrier);
        Assert.Equal(preference.ShippingAddress.RecipientName, order.ShippingAddress!.RecipientName);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CheckoutRequest CreateCheckoutRequest(
        Guid? preferenceId = null,
        DeliveryCarrier carrier = DeliveryCarrier.UPS,
        string provider = "Stripe",
        decimal taxRate = 0.07m) =>
        new()
        {
            DeliveryPreferenceId = preferenceId,
            Carrier = carrier,
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
        Guid? preferenceId = null,
        DeliveryCarrier carrier = DeliveryCarrier.UPS,
        string provider = "Stripe",
        decimal taxRate = 0.07m) =>
        new()
        {
            LotId = lotId,
            TicketsCount = ticketsCount,
            DeliveryPreferenceId = preferenceId,
            Carrier = carrier,
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
        Guid? preferenceId = null,
        DeliveryCarrier carrier = DeliveryCarrier.UPS,
        string provider = "Stripe",
        decimal taxRate = 0.07m) =>
        new()
        {
            LotId = lotId,
            Amount = amount,
            DeliveryPreferenceId = preferenceId,
            Carrier = carrier,
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
}