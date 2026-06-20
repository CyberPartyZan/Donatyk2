namespace Marketplace
{
    public interface ICompensationService
    {
        Task<Guid> Create(Guid orderId, Guid lotId, Money amount);
        Task<Guid> CreateIfNotExists(Guid orderId, Guid lotId, Money amount);
        Task<CompensationDto?> Get(Guid id);
        Task<IReadOnlyCollection<CompensationDto>> GetBySellerId(Guid sellerId, CompensationStatus? status = null);
        Task<CompensationGroupedPageDto> GetAll(int page, int pageSize, CompensationStatus? status = null);
        Task Update(IReadOnlyCollection<Guid> ids, CompensationStatus status);
        Task<int> RequestCompensation(Guid sellerId);
    }
}