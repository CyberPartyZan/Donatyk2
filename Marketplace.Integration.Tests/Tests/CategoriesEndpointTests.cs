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
        var (parent, child) = await SeedCategoryHierarchyAsync();

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
        var category = await SeedCategoryAsync();

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
        var parent = await SeedCategoryAsync("Parent");

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
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var stored = await db.Categories.SingleAsync(c => c.Id == created.Id);
        Assert.Equal(parent.Id, stored.ParentCategoryId);
    }

    [Fact]
    public async Task Put_UpdatesCategory()
    {
        var category = await SeedCategoryAsync("Original");
        var newParent = await SeedCategoryAsync("Updated Parent");

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
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var stored = await db.Categories.SingleAsync(c => c.Id == category.Id);
        Assert.Equal(update.Name, stored.Name);
        Assert.Equal(update.Description, stored.Description);
        Assert.Equal(newParent.Id, stored.ParentCategoryId);
    }

    [Fact]
    public async Task Delete_RemovesCategory()
    {
        var category = await SeedCategoryAsync();

        var response = await _client.DeleteAsync($"/api/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        Assert.False(await db.Categories.AnyAsync(c => c.Id == category.Id));
    }

    private async Task<CategoryEntity> SeedCategoryAsync(string? name = null, Guid? parentId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();

        var unique = Guid.NewGuid().ToString("N");
        var entity = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Category {unique}",
            Description = $"Description for {(name ?? "category")} {unique}",
            ParentCategoryId = parentId
        };

        db.Categories.Add(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    private async Task<(CategoryEntity Parent, CategoryEntity Child)> SeedCategoryHierarchyAsync()
    {
        var parent = await SeedCategoryAsync("Parent Category");
        var child = await SeedCategoryAsync("Child Category", parent.Id);
        return (parent, child);
    }
}