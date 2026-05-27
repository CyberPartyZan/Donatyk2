using System.Net;
using System.Net.Http.Json;
using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marketplace.Integration.Tests;

public class CategoriesEndpointTests : IntegrationTestsBase
{
    public CategoriesEndpointTests(CustomWebApplicationFactory factory)
            : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_ReturnsCategoryHierarchy()
    {
        var (parent, child) = await IntegrationTestsHelper.SeedCategoryHierarchyAsync(_factory.Services);

        var response = await _client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.NotNull(payload);

        var parentDto = payload!.Single(c => c.Id == parent.Id);
        Assert.NotNull(parentDto.SubCategories);
        Assert.Contains(parentDto.SubCategories, sub => sub.Id == child.Id);
    }

    [Fact]
    public async Task Get_ReturnsCategory_WhenExists()
    {
        var category = await IntegrationTestsHelper.SeedCategoryAsync(_factory.Services);

        var response = await _client.GetAsync($"/api/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(payload);
        Assert.Equal(category.Id, payload!.Id);
        Assert.Equal(category.Name, payload.Name);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesCategory()
    {
        var parent = await IntegrationTestsHelper.SeedCategoryAsync(_factory.Services, "Parent");

        var request = new CategoryDto
        {
            Name = "New Category",
            Description = "Child category description",
            ParentId = parent.Id,
            SubCategories = new List<CategoryDto>()
        };

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(parent.Id, created.ParentId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var stored = await db.Categories.SingleAsync(c => c.Id == created.Id);
        Assert.Equal(parent.Id, stored.ParentCategoryId);
    }

    [Fact]
    public async Task Put_UpdatesCategory()
    {
        var category = await IntegrationTestsHelper.SeedCategoryAsync(_factory.Services, "Original");
        var newParent = await IntegrationTestsHelper.SeedCategoryAsync(_factory.Services, "Updated Parent");

        var update = new CategoryDto
        {
            Id = category.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            ParentId = newParent.Id,
            SubCategories = new List<CategoryDto>()
        };

        var response = await _client.PutAsJsonAsync($"/api/categories/{category.Id}", update);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var stored = await db.Categories.SingleAsync(c => c.Id == category.Id);
        Assert.Equal(update.Name, stored.Name);
        Assert.Equal(update.Description, stored.Description);
        Assert.Equal(newParent.Id, stored.ParentCategoryId);
    }

    [Fact]
    public async Task Delete_RemovesCategory()
    {
        var category = await IntegrationTestsHelper.SeedCategoryAsync(_factory.Services);

        var response = await _client.DeleteAsync($"/api/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        Assert.False(await db.Categories.AnyAsync(c => c.Id == category.Id));
    }
}