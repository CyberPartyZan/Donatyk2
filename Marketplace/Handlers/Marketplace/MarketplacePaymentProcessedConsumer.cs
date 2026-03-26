using Marketplace.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    internal class MarketplacePaymentProcessedConsumer : IConsumer<PaymentProcessed>
    {
        private readonly ITicketsService _ticketsService;
        private readonly ILogger<MarketplacePaymentProcessedConsumer> _logger;

        public MarketplacePaymentProcessedConsumer(
            ITicketsService ticketsService,
            ILogger<MarketplacePaymentProcessedConsumer> logger)
        {
            _ticketsService = ticketsService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentProcessed> context)
        {
            var message = context.Message;

            if (!message.Succeeded)
            {
                return;
            }

            await _ticketsService.MarkAsPayedByOrderId(message.OrderId);

            _logger.LogInformation(
                "Marked draw tickets as paid for order {OrderId} after successful payment.",
                message.OrderId);
        }
    }
}