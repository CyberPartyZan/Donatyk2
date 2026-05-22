namespace Marketplace.Payment
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway CreatePaymentGateway(string provider);
    }
}
