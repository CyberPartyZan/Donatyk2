using Marketplace.Abstractions;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    internal class AuctionEndedConsumer : IConsumer<AuctionEnded>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<AuctionEndedConsumer> _logger;

        public AuctionEndedConsumer(
            IOrdersRepository ordersRepository,
            IPaymentGateway paymentGateway,
            ILogger<AuctionEndedConsumer> logger)
        {
            _ordersRepository = ordersRepository;
            _paymentGateway = paymentGateway;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionEnded> context)
        {
            var lotId = context.Message.LotId;

            _logger.LogInformation("AuctionEnded received for lot {LotId}. Looking up winning bid hold order.", lotId);

            var order = await _ordersRepository.GetPaidOrderByLotId(lotId, context.CancellationToken);

            if (order is null)
            {
                _logger.LogWarning(
                    "No paid hold order found for auction lot {LotId}. Nothing to capture.",
                    lotId);
                return;
            }

            // Server-to-server call: capture the previously held amount from the payment gateway
            var captureUrl = await _paymentGateway.CaptureHoldAsync(order, context.CancellationToken);

            _logger.LogInformation(
                "Hold captured for order {OrderId} (lot {LotId}) via {CaptureUrl}.",
                order.Id, lotId, captureUrl);
        }
    }
}