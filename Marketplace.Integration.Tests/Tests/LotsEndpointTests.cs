using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marketplace.Integration.Tests;

public class LotsEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public LotsEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.Scheme);
        await EnsureTestUserExistsAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

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
                AvatarImageUrl = "https://example.com/avatar.png"
            },
            IsActive = true,
            IsCompensationPaid = true,
            CreatedAt = DateTime.UtcNow
        };

        var response = await _client.PutAsJsonAsync($"/api/lots/{seededLot.Id}", updateDto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
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
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var deletedLot = await db.Lots.IgnoreQueryFilters().SingleAsync(l => l.Id == seededLot.Id);

        Assert.True(deletedLot.IsDeleted);
    }

    [Fact]
    public async Task ApproveLot_MarksStageAsApproved()
    {
        var seededLot = await SeedLotAsync(l => l.Stage = LotStage.PendingApproval);

        var response = await _client.PostAsync($"/api/lots/{seededLot.Id}/approve", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var lot = await db.Lots.SingleAsync(l => l.Id == seededLot.Id);

        Assert.Equal(LotStage.Approved, lot.Stage);
        Assert.Null(lot.DeclineReason);
    }

    [Fact]
    public async Task DeclineLot_SetsStageAndReason()
    {
        var seededLot = await SeedLotAsync(l => l.Stage = LotStage.PendingApproval);
        var request = new DeclineLotRequest { Reason = "Missing documentation" };

        var response = await _client.PostAsJsonAsync($"/api/lots/{seededLot.Id}/decline", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var lot = await db.Lots.SingleAsync(l => l.Id == seededLot.Id);

        Assert.Equal(LotStage.Denied, lot.Stage);
        Assert.Equal(request.Reason, lot.DeclineReason);
    }

    private async Task EnsureTestUserExistsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        if (await db.Users.AnyAsync(u => u.Id == TestAuthHandler.UserId))
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = TestAuthHandler.UserId,
            UserName = "integration@test.com",
            NormalizedUserName = "INTEGRATION@TEST.COM",
            Email = "integration@test.com",
            NormalizedEmail = "INTEGRATION@TEST.COM",
            EmailConfirmed = true,
            PasswordHash = "integration-test",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550123",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = "integration-test"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private async Task<LotEntity> SeedLotAsync(Action<LotEntity>? configure = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        var sellerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "seller@test.com",
            NormalizedUserName = "SELLER@TEST.COM",
            Email = "seller@test.com",
            NormalizedEmail = "SELLER@TEST.COM",
            EmailConfirmed = true,
            PasswordHash = "seller-password",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550100",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = "seller-password"
        };

        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Description = "A category for testing."
        };

        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Seller",
            Description = "A seller for testing.",
            Email = "test.seller@example.com",
            PhoneNumber = "+1234567890",
            AvatarImageUrl = string.Empty,
            UserId = sellerUser.Id,
            User = sellerUser,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var lot = new LotEntity
        {
            Id = Guid.NewGuid(),
            Name = "Existing lot",
            Description = "This is an existing lot.",
            Price = new Money(100, Currency.USD),
            Compensation = new Money(50, Currency.USD),
            StockCount = 10,
            Discount = 0,
            Type = LotType.Simple,
            Stage = LotStage.PendingApproval,
            Seller = seller,
            Category = category,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        configure?.Invoke(lot);

        db.Users.Add(sellerUser);
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        return lot;
    }
}