using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace Marketplace.Integration.Tests;

public class ShipmentEndpointTests : IntegrationTestsBase
{
    public ShipmentEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    // ── TakeIntoProcessing ────────────────────────────────────────────────────

    [Fact]
    public async Task TakeIntoProcessing_WhenCreatedShipment_Returns204AndUpdatesStatuses()
    {
        var (order, shipment) = await SeedPaidOrderWithShipmentAsync();

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/take-into-processing", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var updatedShipment = await db.Shipments.SingleAsync(s => s.Id == shipment.Id);
        Assert.Equal(ShipmentStatus.Processing, updatedShipment.Status);
    }

    [Fact]
    public async Task TakeIntoProcessing_WhenShipmentNotFound_Returns404()
    {
        var response = await _client.PutAsync(
            $"/api/shipment/{Guid.NewGuid()}/take-into-processing", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TakeIntoProcessing_WhenAlreadyProcessing_Returns400()
    {
        var (_, shipment) = await SeedPaidOrderWithShipmentAsync();

        await _client.PutAsync($"/api/shipment/{shipment.Id}/take-into-processing", null);

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/take-into-processing", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Shipped ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Shipped_WhenProcessingShipment_Returns204AndUpdatesStatuses()
    {
        var (order, shipment) = await SeedPaidOrderWithShipmentAsync();
        await _client.PutAsync($"/api/shipment/{shipment.Id}/take-into-processing", null);

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/shipped", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var updatedShipment = await db.Shipments.SingleAsync(s => s.Id == shipment.Id);
        Assert.Equal(ShipmentStatus.Shipped, updatedShipment.Status);
    }

    [Fact]
    public async Task Shipped_WhenNotProcessing_Returns400()
    {
        var (_, shipment) = await SeedPaidOrderWithShipmentAsync();

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/shipped", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Shipped_WhenShipmentNotFound_Returns404()
    {
        var response = await _client.PutAsync(
            $"/api/shipment/{Guid.NewGuid()}/shipped", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── InTransit ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task InTransit_WhenShippedShipment_Returns204AndUpdatesStatuses()
    {
        var (order, shipment) = await SeedPaidOrderWithShipmentAsync();
        await _client.PutAsync($"/api/shipment/{shipment.Id}/take-into-processing", null);
        await _client.PutAsync($"/api/shipment/{shipment.Id}/shipped", null);

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/in-transit", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var updatedShipment = await db.Shipments.SingleAsync(s => s.Id == shipment.Id);
        Assert.Equal(ShipmentStatus.InTransit, updatedShipment.Status);
    }

    [Fact]
    public async Task InTransit_WhenNotShipped_Returns400()
    {
        var (_, shipment) = await SeedPaidOrderWithShipmentAsync();

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/in-transit", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InTransit_WhenShipmentNotFound_Returns404()
    {
        var response = await _client.PutAsync(
            $"/api/shipment/{Guid.NewGuid()}/in-transit", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── OutForDelivery ────────────────────────────────────────────────────────

    [Fact]
    public async Task OutForDelivery_WhenInTransitShipment_Returns204AndUpdatesStatuses()
    {
        var (order, shipment) = await SeedPaidOrderWithShipmentAsync();
        await _client.PutAsync($"/api/shipment/{shipment.Id}/take-into-processing", null);
        await _client.PutAsync($"/api/shipment/{shipment.Id}/shipped", null);
        await _client.PutAsync($"/api/shipment/{shipment.Id}/in-transit", null);

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/out-for-delivery", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var updatedShipment = await db.Shipments.SingleAsync(s => s.Id == shipment.Id);
        Assert.Equal(ShipmentStatus.OutForDelivery, updatedShipment.Status);
    }

    [Fact]
    public async Task OutForDelivery_WhenNotInTransit_Returns400()
    {
        var (_, shipment) = await SeedPaidOrderWithShipmentAsync();

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/out-for-delivery", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OutForDelivery_WhenShipmentNotFound_Returns404()
    {
        var response = await _client.PutAsync(
            $"/api/shipment/{Guid.NewGuid()}/out-for-delivery", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Delivered ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delivered_WhenOutForDeliveryShipment_Returns204AndUpdatesStatuses()
    {
        var (order, shipment) = await SeedPaidOrderWithShipmentAsync();
        await _client.PutAsync($"/api/shipment/{shipment.Id}/take-into-processing", null);
        await _client.PutAsync($"/api/shipment/{shipment.Id}/shipped", null);
        await _client.PutAsync($"/api/shipment/{shipment.Id}/in-transit", null);
        await _client.PutAsync($"/api/shipment/{shipment.Id}/out-for-delivery", null);

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/delivered", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var updatedShipment = await db.Shipments.SingleAsync(s => s.Id == shipment.Id);
        Assert.Equal(ShipmentStatus.Delivered, updatedShipment.Status);
    }

    [Fact]
    public async Task Delivered_WhenNotOutForDelivery_Returns400()
    {
        var (_, shipment) = await SeedPaidOrderWithShipmentAsync();

        var response = await _client.PutAsync(
            $"/api/shipment/{shipment.Id}/delivered", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delivered_WhenShipmentNotFound_Returns404()
    {
        var response = await _client.PutAsync(
            $"/api/shipment/{Guid.NewGuid()}/delivered", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Full lifecycle ────────────────────────────────────────────────────────

    [Fact]
    public async Task FullLifecycle_ShipmentProgressesThroughAllStatuses()
    {
        var (order, shipment) = await SeedPaidOrderWithShipmentAsync();

        async Task AssertTransitionAsync(
            string endpoint,
            ShipmentStatus expectedShipment)
        {
            var r = await _client.PutAsync($"/api/shipment/{shipment.Id}/{endpoint}", null);
            Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);

            using var s = _factory.Services.CreateScope();
            var db = s.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

            var sh = await db.Shipments.SingleAsync(x => x.Id == shipment.Id);
            Assert.Equal(expectedShipment, sh.Status);
        }

        await AssertTransitionAsync("take-into-processing", ShipmentStatus.Processing);
        await AssertTransitionAsync("shipped",              ShipmentStatus.Shipped);
        await AssertTransitionAsync("in-transit",           ShipmentStatus.InTransit);
        await AssertTransitionAsync("out-for-delivery",     ShipmentStatus.OutForDelivery);
        await AssertTransitionAsync("delivered",            ShipmentStatus.Delivered);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<(OrderEntity Order, ShipmentEntity Shipment)> SeedPaidOrderWithShipmentAsync()
    {
        var order = await IntegrationTestsHelper.SeedOrderAsync(
            _factory.Services, TestAuthHandler.UserId, OrderStatus.Paid);

        var shipment = await IntegrationTestsHelper.SeedShipmentAsync(
            _factory.Services, order.Id);

        // Link the shipment back to the order
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var trackedOrder = await db.Orders.SingleAsync(o => o.Id == order.Id);
        trackedOrder.ShipmentId = shipment.Id;
        await db.SaveChangesAsync();

        return (trackedOrder, shipment);
    }
}