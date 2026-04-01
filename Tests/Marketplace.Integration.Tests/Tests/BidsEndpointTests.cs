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
        var lot = await SeedAuctionLotAsync();

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

    private async Task<LotEntity> SeedAuctionLotAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"seller-{Guid.NewGuid():N}@example.com",
            NormalizedUserName = $"SELLER-{Guid.NewGuid():N}@EXAMPLE.COM",
            Email = $"seller-{Guid.NewGuid():N}@example.com",
            NormalizedEmail = $"SELLER-{Guid.NewGuid():N}@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            Password = "seller-password"
        };

        var category = new CategoryEntity { Id = Guid.NewGuid(), Name = "Bids", Description = "Bids category" };
        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Seller",
            Description = "Seller",
            Email = $"seller-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+15555550111",
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow
        };

        var lot = new LotEntity
        {
            Id = Guid.NewGuid(),
            Name = "Auction lot",
            Description = "Auction lot",
            Price = new Money(100, Currency.USD),
            Compensation = new Money(60, Currency.USD),
            StockCount = 1,
            Type = LotType.Auction,
            Stage = LotStage.Approved,
            Seller = seller,
            Category = category,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            EndOfAuction = DateTime.UtcNow.AddHours(2),
            AuctionStepPercent = 5
        };

        db.Users.Add(user);
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        return lot;
    }
}