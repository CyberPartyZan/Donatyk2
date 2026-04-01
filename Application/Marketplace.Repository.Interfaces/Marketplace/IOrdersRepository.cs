namespace Marketplace.Repository
{
    public interface IOrdersRepository
    {
        Task<Guid> Create(Order order);
        Task<Guid> MarkPaid(Guid orderId, string provider, string paymentReference);
        Task<Guid> MarkPaid(Guid orderId);
    }
}