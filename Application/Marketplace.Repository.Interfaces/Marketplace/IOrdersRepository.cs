namespace Marketplace.Repository
{
    public interface IOrdersRepository
    {
        Task<Guid> Create(Order order);
        Task<Order?> GetById(Guid orderId);
        Task<Order?> GetPaidOrderByLotId(Guid lotId, CancellationToken cancellationToken = default);
        Task Update(Order order);
    }
}