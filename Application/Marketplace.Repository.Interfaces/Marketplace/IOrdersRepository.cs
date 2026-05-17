namespace Marketplace.Repository
{
    public interface IOrdersRepository
    {
        Task<Guid> Create(Order order);
        Task<Order?> GetById(Guid orderId);
        Task<Order?> GetPaidOrderByLotId(Guid lotId, CancellationToken cancellationToken = default);
        Task<Guid> MarkPaid(Guid orderId, string provider, string paymentReference);
        Task<Guid> MarkPaid(Guid orderId);
        Task Cancel(Guid orderId);
    }
}