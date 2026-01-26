using Donatyk2.Server.Models;
using Donatyk2.Server.Services.Interfaces;
using Donatyk2.Server.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Donatyk2.Server.Services.Payments
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
            {
                url += $"&returnUrl={Uri.EscapeDataString(paymentInfo.ReturnUrl)}";
            }

            _logger.LogInformation("Generated payment url {Url} for order {OrderId}", url, order.Id);

            return Task.FromResult(url);
        }
    }
}