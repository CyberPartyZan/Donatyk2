using Donatyk2.Server.Models;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface ISellersRepository
    {
        Task<IEnumerable<Seller>> GetAll(string? search, int page, int pageSize);
        Task<Seller?> GetById(Guid id);
        Task<Guid> Create(Seller seller);
        Task Update(Seller seller);
        Task Delete(Guid id);
    }
}
