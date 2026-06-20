namespace Marketplace.Repository
{
    public interface ICompensationRepository
    {
        Task<Guid> Create(Compensation compensation);
        Task<Compensation?> Get(Guid id);
        Task<IReadOnlyCollection<CompensationReadModel>> GetBySellerId(Guid sellerId, CompensationStatus? status = null);
        Task<(IReadOnlyCollection<CompensationReadModel> Items, int TotalGroups)> GetAll(int page, int pageSize, CompensationStatus? status = null);
        Task Update(IReadOnlyCollection<Compensation> compensations);
        Task<bool> Exists(Guid orderId, Guid lotId);
    }
}