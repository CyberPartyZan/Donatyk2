using System.Net;
using System.Net.Http.Json;
using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
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

        db.Lots.Add(new LotEntity { Id = Guid.NewGuid(), Name = "Existing lot" });
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetLots_ReturnsSeededLot()
    {
        var response = await _client.GetAsync("/api/lots");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<LotDto>>();
        Assert.Contains(payload!, lot => lot.Name == "Existing lot");
    }
}