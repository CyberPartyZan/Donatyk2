using Donatyk2.Server.Models;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface IOrdersRepository
    {
        Task<Guid> Create(Order order);
        Task<Guid> MarkPaid(Guid orderId, string provider, string paymentReference);
    }
}