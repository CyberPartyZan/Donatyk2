namespace Marketplace
{
    public interface IPaymentGateway
    {
        Task<string> CreatePaymentUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default);
        Task<string> CreatePaymentAuctionUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default);
        Task<string> CreatePaymentDrawUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default);
        Task<string> CaptureHoldAsync(Order order, CancellationToken cancellationToken = default);
        Task ReleaseHoldAsync(Order order, CancellationToken cancellationToken = default);
    }
}