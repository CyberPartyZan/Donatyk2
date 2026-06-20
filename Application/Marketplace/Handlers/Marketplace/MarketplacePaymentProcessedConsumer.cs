using Marketplace.Abstractions;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    internal class MarketplacePaymentProcessedConsumer : IConsumer<PaymentProcessed>
    {
        private readonly IOrdersService _ordersService;
        private readonly ITicketsService _ticketsService;
        private readonly ILogger<MarketplacePaymentProcessedConsumer> _logger;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILotsRepository _lotsRepository;
        private readonly ICompensationService _compensationService;

        public MarketplacePaymentProcessedConsumer(
            IOrdersService ordersService,
            ITicketsService ticketsService,
            ILogger<MarketplacePaymentProcessedConsumer> logger,
            IOrdersRepository ordersRepository,
            ILotsRepository lotsRepository,
            ICompensationService compensationService)
        {
            _ordersService = ordersService;
            _ticketsService = ticketsService;
            _logger = logger;
            _ordersRepository = ordersRepository;
            _lotsRepository = lotsRepository;
            _compensationService = compensationService;
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
            var order = await _ordersRepository.GetById(message.OrderId);
            if (order is null) return;

            foreach (var item in order.Items)
            {
                var lot = await _lotsRepository.GetLotById(item.LotId);
                if (lot is null || lot is DrawLot || lot is AuctionLot) continue;

                var amount = new Money(lot.Compensation.Amount * item.Quantity, lot.Compensation.Currency);
                await _compensationService.CreateIfNotExists(order.Id, lot.Id, amount);
            }

            _logger.LogInformation(
                "Marked order {OrderId} and draw tickets as paid after successful payment.",
                message.OrderId);
        }
    }
}