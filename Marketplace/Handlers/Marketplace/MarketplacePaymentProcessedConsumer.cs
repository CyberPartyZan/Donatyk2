using Marketplace.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    internal class MarketplacePaymentProcessedConsumer : IConsumer<PaymentProcessed>
    {
        private readonly IOrdersService _ordersService;
        private readonly ITicketsService _ticketsService;
        private readonly ILogger<MarketplacePaymentProcessedConsumer> _logger;

        public MarketplacePaymentProcessedConsumer(
            IOrdersService ordersService,
            ITicketsService ticketsService,
            ILogger<MarketplacePaymentProcessedConsumer> logger)
        {
            _ordersService = ordersService;
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

            await _ordersService.MarkPaid(message.OrderId);
            await _ticketsService.MarkAsPayedByOrderId(message.OrderId);

            _logger.LogInformation(
                "Marked order {OrderId} and draw tickets as paid after successful payment.",
                message.OrderId);
        }
    }
}