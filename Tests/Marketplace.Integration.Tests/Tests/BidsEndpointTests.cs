using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Marketplace.Integration.Tests;

public class BidsEndpointTests : IntegrationTestsBase
{
    public BidsEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task PlaceBid_CreatesBidAndUpdatesLotPrice()
    {
        var lot = await IntegrationTestsHelper.SeedLotAsync(
            _factory.Services,
            stockCount: 1,
            type: LotType.Auction,
            stage: LotStage.Approved,
            configure: l =>
            {
                l.Name = "Auction lot";
                l.Description = "Auction lot";
                l.Price = new Money(100, Currency.USD);
                l.Compensation = new Money(60, Currency.USD);
                l.EndOfAuction = DateTime.UtcNow.AddHours(2);
                l.AuctionStepPercent = 5;
            });

        var response = await _client.PostAsJsonAsync($"/api/bids/{lot.Id}", new PlaceBidRequest
        {
            Amount = new Money(120, Currency.USD)
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var bid = await db.BidHistory.SingleAsync(b => b.AuctionId == lot.Id);
        Assert.Equal(TestAuthHandler.UserId, bid.BidderId);
        Assert.Equal(120, bid.Amount.Amount);

        var updatedLot = await db.Lots.SingleAsync(l => l.Id == lot.Id);
        Assert.Equal(120, updatedLot.Price.Amount);
    }
}