using Donatyk2.Server.Data;
using Donatyk2.Server.Repositories.Interfaces;
using Marketplace.Abstractions.Models;
using Marketplace.Repository.MSSql.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Donatyk2.Server.Repositories
{
    internal class CategoriesRepository : ICategoriesRepository
    {
        private readonly DonatykDbContext _dbContext;

        public CategoriesRepository(DonatykDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return BuildHierarchy(entities);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dictionary = MaterializeDictionary(entities);
            return dictionary.TryGetValue(id, out var category) ? category : null;
        }

        public async Task<Category> CreateAsync(Category category, Guid? parentCategoryId, CancellationToken cancellationToken = default)
        {
            await EnsureParentExists(parentCategoryId, cancellationToken, category.Id);

            var entity = new CategoryEntity
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = parentCategoryId
            };

            _dbContext.Categories.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken) ?? category;
        }

        public async Task UpdateAsync(Category category, Guid? parentCategoryId, CancellationToken cancellationToken = default)
        {
            await EnsureParentExists(parentCategoryId, cancellationToken, category.Id);

            var entity = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);
            if (entity is null)
            {
                throw new KeyNotFoundException($"Category '{category.Id}' was not found.");
            }

            entity.Name = category.Name;
            entity.Description = category.Description;
            entity.ParentCategoryId = parentCategoryId;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (entity is null)
            {
                return;
            }

            _dbContext.Categories.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task EnsureParentExists(Guid? parentCategoryId, CancellationToken cancellationToken, Guid? childId = null)
        {
            if (!parentCategoryId.HasValue)
            {
                return;
            }

            if (childId.HasValue && childId.Value == parentCategoryId.Value)
            {
                throw new InvalidOperationException("Category cannot be its own parent.");
            }

            var exists = await _dbContext.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Id == parentCategoryId.Value, cancellationToken);

            if (!exists)
            {
                throw new KeyNotFoundException($"Parent category '{parentCategoryId}' was not found.");
            }
        }

        private static IReadOnlyCollection<Category> BuildHierarchy(IEnumerable<CategoryEntity> entities)
        {
            var dictionary = MaterializeDictionary(entities);
            return dictionary.Values
                .Where(category => category.ParentCategory is null)
                .ToList()
                .AsReadOnly();
        }

        private static Dictionary<Guid, Category> MaterializeDictionary(IEnumerable<CategoryEntity> entities)
        {
            var dictionary = entities.ToDictionary(
                entity => entity.Id,
                entity => new Category(entity.Id, entity.Name, entity.Description));

            foreach (var entity in entities)
            {
                if (entity.ParentCategoryId.HasValue &&
                    dictionary.TryGetValue(entity.ParentCategoryId.Value, out var parent) &&
                    dictionary.TryGetValue(entity.Id, out var child))
                {
                    parent.AddSubCategory(child);
                }
            }

            return dictionary;
        }
    }
}