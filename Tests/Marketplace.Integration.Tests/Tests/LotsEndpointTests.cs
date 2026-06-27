using System.Net;
using System.Net.Http.Json;
using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marketplace.Integration.Tests;

public class LotsEndpointTests : IntegrationTestsBase
{
    public LotsEndpointTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLots_ReturnsSeededLot()
    {
        var seededLot = await SeedLotAsync();

        var response = await _client.GetAsync("/api/lots");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<LotDto>>();
        Assert.Contains(payload!, lot => lot.Id == seededLot.Id && lot.Name == seededLot.Name);
    }

    [Fact]
    public async Task GetLotById_ReturnsExpectedLot()
    {
        var seededLot = await SeedLotAsync();

        var response = await _client.GetAsync($"/api/lots/{seededLot.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LotDto>();
        Assert.Equal(seededLot.Id, payload!.Id);
        Assert.Equal(seededLot.Name, payload.Name);
    }

    [Fact]
    public async Task GetLotById_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.GetAsync($"/api/lots/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutLot_UpdatesLotDetails()
    {
        var seededLot = await SeedLotAsync();
        var updateDto = new LotDto
        {
            Id = seededLot.Id,
            Name = "Updated lot",
            Description = "Updated description",
            Price = new Money(200, Currency.USD),
            Compensation = new Money(120, Currency.USD),
            StockCount = 5,
            Discount = 10,
            Type = LotType.Simple,
            Stage = LotStage.Approved,
            Seller = new SellerDto
            {
                Id = seededLot.Seller.Id,
                Name = "Updated Seller",
                Description = "Updated seller description",
                Email = "updated.seller@example.com",
                PhoneNumber = "+15555550101",
                Avatar = seededLot.Seller.Avatar is not null ? new BlobDto
                {
                    Id = seededLot.Seller.Avatar.Id,
                    FilePath = seededLot.Seller.Avatar.FilePath,
                    Key = seededLot.Seller.Avatar.Key,
                    FileName = seededLot.Seller.Avatar.FileName
                } : null
            },
            Category = new CategoryDto
            {
                Id = seededLot.Category.Id,
                Name = seededLot.Category.Name,
                Description = seededLot.Category.Description,
                ParentId = seededLot.Category.ParentCategoryId
            },
            IsActive = true,
            IsCompensationPaid = true,
            CreatedAt = DateTime.UtcNow
        };

        var response = await _client.PutAsJsonAsync($"/api/lots/{seededLot.Id}", updateDto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var updatedLot = await db.Lots.Include(l => l.Seller).SingleAsync(l => l.Id == seededLot.Id);

        Assert.Equal(updateDto.Name, updatedLot.Name);
        Assert.Equal(updateDto.Description, updatedLot.Description);
        Assert.Equal(updateDto.Price.Amount, updatedLot.Price.Amount);
        Assert.Equal(LotStage.Approved, updatedLot.Stage);
        Assert.Equal("Updated Seller", updatedLot.Seller.Name);
    }

    [Fact]
    public async Task DeleteLot_SoftDeletesEntity()
    {
        var seededLot = await SeedLotAsync();

        var response = await _client.DeleteAsync($"/api/lots/{seededLot.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var deletedLot = await db.Lots.IgnoreQueryFilters().SingleAsync(l => l.Id == seededLot.Id);

        Assert.True(deletedLot.IsDeleted);
    }

    [Fact]
    public async Task ApproveLot_MarksStageAsApproved()
    {
        var seededLot = await SeedLotAsync(configure: l => l.Stage = LotStage.PendingApproval);

        var response = await _client.PostAsync($"/api/lots/{seededLot.Id}/approve", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var lot = await db.Lots.SingleAsync(l => l.Id == seededLot.Id);

        Assert.Equal(LotStage.Approved, lot.Stage);
        Assert.Null(lot.DeclineReason);
    }

    [Fact]
    public async Task DeclineLot_SetsStageAndReason()
    {
        var seededLot = await SeedLotAsync(configure: l => l.Stage = LotStage.PendingApproval);
        var request = new DeclineLotRequest { Reason = "Missing documentation" };

        var response = await _client.PostAsJsonAsync($"/api/lots/{seededLot.Id}/decline", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var lot = await db.Lots.SingleAsync(l => l.Id == seededLot.Id);

        Assert.Equal(LotStage.Denied, lot.Stage);
        Assert.Equal(request.Reason, lot.DeclineReason);
    }

    [Fact]
    public async Task GetLots_WithStageFilter_ReturnsOnlyRequestedStage()
    {
        var approvedLot = await SeedLotAsync(configure: l => l.Stage = LotStage.Approved);
        var deniedLot = await SeedLotAsync(configure: l => l.Stage = LotStage.Denied);

        var response = await _client.GetAsync("/api/lots?stage=Approved");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<LotDto>>();
        Assert.NotNull(payload);
        Assert.Contains(payload!, lot => lot.Id == approvedLot.Id);
        Assert.DoesNotContain(payload!, lot => lot.Id == deniedLot.Id);
    }

    [Fact]
    public async Task GetLots_WithSearchTextAndTypeFilter_ReturnsOnlyMatchingLots()
    {
        var matching = await SeedLotAsync(configure: l =>
        {
            l.Name = "Admin Search Auction Match";
            l.Type = LotType.Auction;
            l.Stage = LotStage.Approved;
            l.EndOfAuction = DateTime.UtcNow.AddDays(1);
            l.AuctionStepPercent = 5;
        });

        await SeedLotAsync(configure: l =>
        {
            l.Name = "Admin Search Auction Match But Simple";
            l.Type = LotType.Simple;
            l.Stage = LotStage.Approved;
        });

        await SeedLotAsync(configure: l =>
        {
            l.Name = "Completely Different Name";
            l.Type = LotType.Auction;
            l.Stage = LotStage.Approved;
            l.EndOfAuction = DateTime.UtcNow.AddDays(1);
            l.AuctionStepPercent = 5;
        });

        var response = await _client.GetAsync("/api/lots?searchText=Admin%20Search%20Auction%20Match&type=Auction");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<LotDto>>();
        Assert.NotNull(payload);

        Assert.Contains(payload!, lot => lot.Id == matching.Id);
        Assert.DoesNotContain(payload!, lot => lot.Name.Contains("But Simple", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(payload!, lot => lot.Name.Contains("Completely Different", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetLots_SetsTotalCountHeader_ForSameFilter()
    {
        var marker = $"count-filter-{Guid.NewGuid():N}";

        await SeedLotAsync(configure: l =>
        {
            l.Name = $"{marker}-approved";
            l.Stage = LotStage.Approved;
        });

        await SeedLotAsync(configure: l =>
        {
            l.Name = $"{marker}-denied";
            l.Stage = LotStage.Denied;
        });

        var response = await _client.GetAsync($"/api/lots?searchText={marker}&stage=Approved&pageNumber=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Total-Count", out var values));

        var totalCount = int.Parse(values!.Single());
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task GetStatistic_ReturnsLotCounters()
    {
        var seller = await IntegrationTestsHelper.SeedSellerAsync(_factory.Services);

        await IntegrationTestsHelper.SeedLotAsync(_factory.Services, sellerEntry: seller, configure: l =>
        {
            l.Stage = LotStage.Approved;
            l.IsActive = true;
        });

        await IntegrationTestsHelper.SeedLotAsync(_factory.Services, sellerEntry: seller, configure: l =>
        {
            l.Stage = LotStage.PendingApproval;
            l.IsActive = true;
        });

        await IntegrationTestsHelper.SeedLotAsync(_factory.Services, sellerEntry: seller, configure: l =>
        {
            l.Stage = LotStage.Denied;
            l.IsActive = false;
        });

        var response = await _client.GetAsync($"/api/lots/statistics?sellerId={seller.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LotStatisticsDto>();
        Assert.NotNull(payload);
        Assert.Equal(3, payload!.Total);
        Assert.Equal(1, payload.Approved);
        Assert.Equal(1, payload.Pending);
        Assert.Equal(2, payload.Active);
    }

    private Task<LotEntity> SeedLotAsync(Action<LotEntity>? configure = null) =>
        IntegrationTestsHelper.SeedLotAsync(_factory.Services, configure: configure);
}