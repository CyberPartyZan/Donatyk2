namespace Marketplace
{
    public interface ISellersService
    {
        Task<IEnumerable<SellerDto>> GetAll(string search, int page, int pageSize);
        Task<SellerDto?> GetById(Guid id);
        Task<Guid> Create(SellerDto seller);
        Task Update(Guid id, SellerDto seller);
        Task Delete(Guid id);
        Task DeleteByUserId(Guid id);
    }
}
