namespace Marketplace
{
    public interface IOrdersService
    {
        Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request);
        Task HandlePaymentWebhookAsync(PaymentWebhookRequest request);
    }
}