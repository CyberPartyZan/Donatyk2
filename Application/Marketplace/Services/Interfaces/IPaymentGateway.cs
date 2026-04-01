namespace Marketplace
{
    public interface IPaymentGateway
    {
        Task<string> CreatePaymentUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default);
        Task<string> CreatePaymentHoldUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default);
    }
}