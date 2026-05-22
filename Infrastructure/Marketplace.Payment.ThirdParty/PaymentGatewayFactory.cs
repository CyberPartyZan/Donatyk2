using Microsoft.Extensions.Logging;
using Stripe;

namespace Marketplace.Payment
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IStripeClient _stripeClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<PaymentGatewayFactory> _logger;

        public PaymentGatewayFactory(
            IStripeClient stripeClient,
            ILoggerFactory loggerFactory)
        {
            _stripeClient = stripeClient;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<PaymentGatewayFactory>();
        }

        public IPaymentGateway CreatePaymentGateway(string provider)
        {
            _logger.LogDebug("Creating payment gateway for provider '{Provider}'.", provider);

            return provider switch
            {
                "Stripe" => new StripePaymentGateway(
                    _stripeClient,
                    _loggerFactory.CreateLogger<StripePaymentGateway>()),
                _ => throw new NotSupportedException($"Payment provider '{provider}' is not supported.")
            };
        }
    }
}