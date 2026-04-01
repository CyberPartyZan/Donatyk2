namespace Marketplace
{
    public interface ICategoriesService
    {
        Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CategoryDto> CreateAsync(CategoryDto category, CancellationToken cancellationToken = default);
        Task UpdateAsync(Guid id, CategoryDto category, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}