using Marketplace.Abstractions.Models;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface ICategoriesRepository
    {
        Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Category> CreateAsync(Category category, Guid? parentCategoryId, CancellationToken cancellationToken = default);
        Task UpdateAsync(Category category, Guid? parentCategoryId, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}