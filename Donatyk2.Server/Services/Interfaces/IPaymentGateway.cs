using Donatyk2.Server.Models;
using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Services.Interfaces
{
    public interface IPaymentGateway
    {
        Task<string> CreatePaymentUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default);
    }
}