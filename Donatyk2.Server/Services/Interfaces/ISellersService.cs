using Donatyk2.Server.Dto;

namespace Donatyk2.Server.Services.Interfaces
{
    public interface ISellersService
    {
        Task<IEnumerable<SellerDto>> GetAll(string search, int page, int pageSize);
        Task<SellerDto?> GetById(Guid id);
        Task<Guid> Create(SellerDto seller);
        Task Update(Guid id, SellerDto seller);
        Task Delete(Guid id);
    }
}
