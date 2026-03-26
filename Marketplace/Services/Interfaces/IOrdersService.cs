namespace Marketplace
{
    public interface IOrdersService
    {
        Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request);
        Task<CheckoutResponse> CheckoutDrawAsync(CheckoutDrawRequest request);
        Task HandlePaymentWebhookAsync(PaymentWebhookRequest request);
    }
}