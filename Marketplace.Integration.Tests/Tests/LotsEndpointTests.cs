using System.Net;
using System.Net.Http.Json;
using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Enums;
using Donatyk2.Server.ValueObjects;
using Marketplace.Repository.MSSql.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        var category = new CategoryEntity { Id = Guid.NewGuid(), Name = "Test Category", Description = "A category for testing." };
        var user = new ApplicationUser { 
            Id = Guid.NewGuid(), 
            UserName = "testuser", 
            Email = "testuser@example.com",
            Password = "hashedpassword", 
            CreatedAt = DateTime.UtcNow,
        };
        var seller = new SellerEntity { 
            Id = Guid.NewGuid(), 
            Name = "Test Seller", 
            Email = "test.seller@example.com", 
            Description = "A seller for testing.",
            PhoneNumber = "+1234567890",
            User = user,
        };
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(new LotEntity { 
            Id = Guid.NewGuid(), 
            Name = "Existing lot", 
            Description = "This is an existing lot.", 
            Price = new Money (100, Currency.USD), 
            Compensation = new Money(50, Currency.USD), 
            Category = category, 
            Seller = seller,
        });
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetLots_ReturnsSeededLot()
    {
        var response = await _client.GetAsync("/api/lots");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<LotDto>>();
        Assert.Contains(payload!, lot => lot.Name == "Existing lot" && lot.Description == "This is an existing lot.");
    }
}