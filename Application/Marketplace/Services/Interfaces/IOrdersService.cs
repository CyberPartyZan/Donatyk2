namespace Marketplace
{
    public interface IOrdersService
    {
        Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request);
        Task<CheckoutResponse> CheckoutDrawAsync(CheckoutDrawRequest request);
        Task<CheckoutResponse> CheckoutAuctionAsync(CheckoutAuctionRequest request);
        Task HandlePaymentWebhookAsync(PaymentWebhookRequest request);
        Task HandleDrawPaymentWebhookAsync(DrawPaymentWebhookRequest request);
        Task<Guid> MarkPaid(Guid orderId);
    }
}