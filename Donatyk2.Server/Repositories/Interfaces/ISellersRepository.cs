using Donatyk2.Server.Data;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface ISellersRepository
    {
        Task<IEnumerable<SellerEntity>> GetAll(string search, int page, int pageSize);
        Task<SellerEntity?> GetById(Guid id);
        Task<Guid> Create(SellerEntity seller); 
        Task Update(SellerEntity seller);
        Task Delete(Guid id);
    }
}
