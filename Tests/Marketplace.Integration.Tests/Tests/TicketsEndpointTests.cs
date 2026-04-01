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

        var category = new CategoryEntity { Id = Guid.NewGuid(), Name = "Tickets", Description = "Tickets category" };
        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Seller",
            Description = "Seller",
            Email = $"seller-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+15555550112",
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow
        };

        var lot = new LotEntity
        {
            Id = Guid.NewGuid(),
            Name = "Draw lot",
            Description = "Draw lot",
            Price = new Money(100, Currency.USD),
            Compensation = new Money(60, Currency.USD),
            StockCount = 1,
            Type = LotType.Draw,
            Stage = LotStage.Approved,
            Seller = seller,
            Category = category,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            TicketPrice = new Money(10, Currency.USD),
            TicketsSold = 0,
            IsDrawn = false
        };

        db.Users.Add(user);
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        return lot;
    }
}