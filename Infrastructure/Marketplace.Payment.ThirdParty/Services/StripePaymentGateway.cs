using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Marketplace.Payment
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly IStripeClient _stripeClient;
        private readonly ILogger<StripePaymentGateway> _logger;

        public StripePaymentGateway(
            IStripeClient stripeClient,
            ILogger<StripePaymentGateway> logger)
        {
            _stripeClient = stripeClient;
            _logger = logger;
        }

        public async Task<string> CreatePaymentUrlAsync(
            Order order,
            PaymentInfo paymentInfo,
            CancellationToken cancellationToken = default)
        {
            var session = await CreateCheckoutSessionAsync(
                order,
                paymentInfo,
                captureMethod: null,
                cancellationToken);

            _logger.LogInformation(
                "Created Stripe Checkout Session {SessionId} for order {OrderId}",
                session.Id, order.Id);

            return session.Url;
        }

        public async Task<string> CreatePaymentAuctionUrlAsync(
            Order order,
            PaymentInfo paymentInfo,
            CancellationToken cancellationToken = default)
        {
            var session = await CreateCheckoutSessionAsync(
                order,
                paymentInfo,
                captureMethod: "manual",
                cancellationToken);

            _logger.LogInformation(
                "Created Stripe Checkout Session (hold) {SessionId} for auction order {OrderId}",
                session.Id, order.Id);

            return session.Url;
        }

        public async Task<string> CreatePaymentDrawUrlAsync(
            Order order,
            PaymentInfo paymentInfo,
            CancellationToken cancellationToken = default)
        {
            var session = await CreateCheckoutSessionAsync(
                order,
                paymentInfo,
                captureMethod: null,
                cancellationToken);

            _logger.LogInformation(
                "Created Stripe Checkout Session (draw) {SessionId} for order {OrderId}",
                session.Id, order.Id);

            return session.Url;
        }

        public async Task<string> CaptureHoldAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            var paymentIntentId = order.PaymentInfo.Reference
                ?? throw new InvalidOperationException(
                    $"Order {order.Id} has no payment reference (PaymentIntent ID) to capture.");

            var service = new PaymentIntentService(_stripeClient);
            var intent = await service.CaptureAsync(
                paymentIntentId,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Captured PaymentIntent {PaymentIntentId} for order {OrderId}. Status: {Status}",
                intent.Id, order.Id, intent.Status);

            return intent.Id;
        }

        public async Task ReleaseHoldAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            var paymentIntentId = order.PaymentInfo.Reference
                ?? throw new InvalidOperationException(
                    $"Order {order.Id} has no payment reference (PaymentIntent ID) to release.");

            var service = new PaymentIntentService(_stripeClient);
            var intent = await service.CancelAsync(
                paymentIntentId,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Released (cancelled) PaymentIntent {PaymentIntentId} for order {OrderId}. Status: {Status}",
                intent.Id, order.Id, intent.Status);
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private async Task<Session> CreateCheckoutSessionAsync(
            Order order,
            PaymentInfo paymentInfo,
            string? captureMethod,
            CancellationToken cancellationToken)
        {
            var lineItems = order.Items.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = CurrencyToStripe(order.Total.Currency),
                    UnitAmountDecimal = ToStripeAmount(item.UnitPrice.Amount),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.NameSnapshot
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var baseReturnUrl = paymentInfo.ReturnUrl ?? "https://payments.local";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{baseReturnUrl}?orderId={order.Id}&isSuccess=true",
                CancelUrl  = $"{baseReturnUrl}?orderId={order.Id}&isSuccess=false",
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = order.Id.ToString()
                }
            };

            if (captureMethod is not null)
            {
                options.PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    CaptureMethod = captureMethod,
                    Metadata = new Dictionary<string, string>
                    {
                        ["orderId"] = order.Id.ToString()
                    }
                };
            }

            var service = new SessionService(_stripeClient);
            return await service.CreateAsync(options, cancellationToken: cancellationToken);
        }

        private static string CurrencyToStripe(Currency currency) =>
            currency switch
            {
                Currency.USD => "usd",
                Currency.EUR => "eur",
                Currency.UAH => "uah",
                _ => throw new NotSupportedException($"Currency {currency} is not supported by Stripe gateway.")
            };

        /// <summary>
        /// Stripe accepts amounts in the smallest currency unit (cents).
        /// </summary>
        private static long ToStripeAmount(decimal amount) =>
            (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
    }
}