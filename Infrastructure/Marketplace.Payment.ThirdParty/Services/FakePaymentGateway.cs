using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marketplace.Payment
{
    public class FakePaymentGateway : IPaymentGateway
    {
        private readonly ILogger<FakePaymentGateway> _logger;
        private readonly string _baseUrl;

        public FakePaymentGateway(IConfiguration configuration, ILogger<FakePaymentGateway> logger)
        {
            _logger = logger;
            _baseUrl = configuration.GetValue<string>("Payments:BaseUrl") ?? "https://payments.local";
        }

        public Task<string> CreatePaymentUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl.TrimEnd('/')}/checkout?orderId={order.Id}&amount={order.Total.Amount}&currency={order.Total.Currency}";

            if (!string.IsNullOrWhiteSpace(paymentInfo.ReturnUrl))
                url += $"&returnUrl={Uri.EscapeDataString(paymentInfo.ReturnUrl)}";

            _logger.LogInformation("Generated payment url {Url} for order {OrderId}", url, order.Id);

            return Task.FromResult(url);
        }

        public Task<string> CreatePaymentAuctionUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl.TrimEnd('/')}/hold?orderId={order.Id}&amount={order.Total.Amount}&currency={order.Total.Currency}";

            if (!string.IsNullOrWhiteSpace(paymentInfo.ReturnUrl))
                url += $"&returnUrl={Uri.EscapeDataString(paymentInfo.ReturnUrl)}";

            _logger.LogInformation("Generated payment hold url {Url} for order {OrderId}", url, order.Id);

            return Task.FromResult(url);
        }

        public Task<string> CreatePaymentDrawUrlAsync(Order order, PaymentInfo paymentInfo, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl.TrimEnd('/')}/draw-checkout?orderId={order.Id}&amount={order.Total.Amount}&currency={order.Total.Currency}";

            if (!string.IsNullOrWhiteSpace(paymentInfo.ReturnUrl))
                url += $"&returnUrl={Uri.EscapeDataString(paymentInfo.ReturnUrl)}";

            _logger.LogInformation("Generated draw payment url {Url} for order {OrderId}", url, order.Id);

            return Task.FromResult(url);
        }

        public Task<string> CaptureHoldAsync(Order order, CancellationToken cancellationToken = default)
        {
            // Server-to-server call: capture the previously held amount for the winning bid
            var url = $"{_baseUrl.TrimEnd('/')}/capture?orderId={order.Id}&amount={order.Total.Amount}&currency={order.Total.Currency}";

            _logger.LogInformation("Capturing hold via server-to-server call {Url} for order {OrderId}", url, order.Id);

            // In a real gateway this would be an HTTP POST. FakePaymentGateway returns the URL it would call.
            return Task.FromResult(url);
        }

        public Task ReleaseHoldAsync(Order order, CancellationToken cancellationToken = default)
        {
            // Server-to-server call: release the previously held amount for the outbid bidder
            var url = $"{_baseUrl.TrimEnd('/')}/release?orderId={order.Id}&amount={order.Total.Amount}&currency={order.Total.Currency}";

            _logger.LogInformation("Releasing hold via server-to-server call {Url} for order {OrderId}", url, order.Id);

            return Task.CompletedTask;
        }
    }
}