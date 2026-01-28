using System;
using System.Collections.Generic;
using System.Linq;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Repositories.Interfaces;
using Donatyk2.Server.Services.Interfaces;
using Marketplace.Abstractions.Models;

namespace Donatyk2.Server.Services
{
    public class CategoryService : ICategoriesService
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public CategoryService(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _categoriesRepository.GetAllAsync(cancellationToken);
            return categories.Select(ToDto).ToList();
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await _categoriesRepository.GetByIdAsync(id, cancellationToken);
            return category is null ? null : ToDto(category);
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var category = new Category(
                dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                dto.Name,
                dto.Description);

            if (dto.ParentId.HasValue && dto.ParentId.Value == category.Id)
            {
                throw new InvalidOperationException("Category cannot be its own parent.");
            }

            var created = await _categoriesRepository.CreateAsync(category, dto.ParentId, cancellationToken);
            return ToDto(created);
        }

        public async Task UpdateAsync(Guid id, CategoryDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.ParentId.HasValue && dto.ParentId.Value == id)
            {
                throw new InvalidOperationException("Category cannot be its own parent.");
            }

            var category = new Category(id, dto.Name, dto.Description);
            await _categoriesRepository.UpdateAsync(category, dto.ParentId, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _categoriesRepository.DeleteAsync(id, cancellationToken);
        }

        private static CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentCategory?.Id,
                SubCategories = category.SubCategories.Select(ToDto).ToList()
            };
        }
    }
}