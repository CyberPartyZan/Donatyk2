using Donatyk2.Server.Dto.Orders;

namespace Donatyk2.Server.Services.Interfaces
{
    public interface IOrdersService
    {
        Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request);
        Task HandlePaymentWebhookAsync(PaymentWebhookRequest request);
    }
}