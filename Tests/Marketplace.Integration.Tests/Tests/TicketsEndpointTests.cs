using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace Marketplace.Integration.Tests;

public class TicketsEndpointTests : IntegrationTestsBase
{
    public TicketsEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateTickets_AddsTicketsAndIncrementsTicketsSold()
    {
        var lot = await SeedDrawLotAsync();

        var response = await _client.PostAsync($"/api/tickets/{lot.Id}?count=2", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var tickets = await db.Tickets.Where(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId).ToListAsync();
        Assert.Equal(2, tickets.Count);

        var updatedLot = await db.Lots.SingleAsync(l => l.Id == lot.Id);
        Assert.Equal(2, updatedLot.TicketsSold);
    }

    [Fact]
    public async Task CreateTickets_SetsCreatedAtAndIsPayedDefaults()
    {
        var lot = await SeedDrawLotAsync();

        var response = await _client.PostAsync($"/api/tickets/{lot.Id}?count=2", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var tickets = await db.Tickets
            .Where(t => t.LotId == lot.Id && t.UserId == TestAuthHandler.UserId)
            .ToListAsync();

        Assert.Equal(2, tickets.Count);
        Assert.All(tickets, t =>
        {
            Assert.False(t.IsPayed);
            Assert.True(t.CreatedAt > DateTime.UtcNow.AddMinutes(-5));
        });
    }

    private async Task<LotEntity> SeedDrawLotAsync()
    {
        var lot = await IntegrationTestsHelper.SeedLotAsync(
            _factory.Services,
            stockCount: 1,
            type: LotType.Draw,
            stage: LotStage.Approved,
            configure: l =>
            {
                l.Name = "Draw lot";
                l.Description = "Draw lot";
                l.TicketPrice = new Money(10, Currency.USD);
                l.TicketsSold = 0;
                l.IsDrawn = false;
            });

        return lot;
    }
}