using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Marketplace.Integration.Tests;

public class DeliveryPreferencesEndpointTests : IntegrationTestsBase
{
    public DeliveryPreferencesEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    // ── Checkout creates a delivery preference ────────────────────────────────

    [Fact]
    public async Task Checkout_WhenNoDeliveryPreferenceId_PersistsDeliveryPreference()
    {
        await SeedCartItemAsync(1, LotType.Simple);

        var request = CreateCheckoutRequest(carrier: DeliveryCarrier.DHL);

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var preference = await db.DeliveryPreferences
            .Include(p => p.ShippingAddress)
            .SingleOrDefaultAsync(p =>
                p.UserId == TestAuthHandler.UserId &&
                p.Carrier == DeliveryCarrier.DHL);

        Assert.NotNull(preference);
        Assert.Equal("Test Buyer", preference!.ShippingAddress.RecipientName);
        Assert.Equal("123 Test Street", preference.ShippingAddress.Line1);
    }

    [Fact]
    public async Task Checkout_WhenSameAddressCheckedOutTwice_DoesNotDuplicatePreference()
    {
        // First checkout
        await SeedCartItemAsync(1, LotType.Simple);
        await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest(carrier: DeliveryCarrier.UPS));

        // Second checkout with identical address + carrier
        await SeedCartItemAsync(1, LotType.Simple);
        var response = await _client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest(carrier: DeliveryCarrier.UPS));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var count = await db.DeliveryPreferences
            .CountAsync(p =>
                p.UserId == TestAuthHandler.UserId &&
                p.Carrier == DeliveryCarrier.UPS);

        Assert.Equal(1, count);
    }

    // ── Checkout using DeliveryPreferenceId ───────────────────────────────────

    [Fact]
    public async Task Checkout_WhenDeliveryPreferenceIdProvided_UsesStoredAddressAndCarrier()
    {
        await SeedCartItemAsync(1, LotType.Simple);

        var preference = await IntegrationTestsHelper.SeedDeliveryPreferencesAsync(
            _factory.Services,
            TestAuthHandler.UserId,
            carrier: DeliveryCarrier.FedEx);

        var request = CreateCheckoutRequest();
        request.DeliveryPreferenceId = preference.Id;

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Order-Id", out var headerValues));
        var orderId = Guid.Parse(headerValues!.Single());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = await db.Orders
            .Include(o => o.ShippingAddress)
            .SingleAsync(o => o.Id == orderId);

        Assert.Equal("Test Recipient", order.ShippingAddress.RecipientName);
        Assert.Equal(DeliveryCarrier.FedEx, order.DeliveryCarrier);
    }

    [Fact]
    public async Task Checkout_WhenDeliveryPreferenceNotFound_ReturnsError()
    {
        await SeedCartItemAsync(1, LotType.Simple);

        var request = CreateCheckoutRequest();
        request.DeliveryPreferenceId = Guid.NewGuid(); // does not exist

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", request);

        // KeyNotFoundException → 500 (or 404 if mapped)
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.InternalServerError);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<(LotEntity Lot, CartItemEntity CartItem, int StockBefore)> SeedCartItemAsync(
        int quantity, LotType type)
    {
        var lot = await IntegrationTestsHelper.SeedLotAsync(
            _factory.Services, stockCount: 10, type: type, stage: LotStage.Approved);
        var cartItem = await IntegrationTestsHelper.SeedCartItemAsync(
            _factory.Services, lot.Id, quantity, TestAuthHandler.UserId);
        return (lot, cartItem, lot.StockCount);
    }

    private static CheckoutRequest CreateCheckoutRequest(
        string provider = "Stripe",
        decimal taxRate = 0.07m,
        DeliveryCarrier carrier = DeliveryCarrier.UPS) =>
        new()
        {
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
}