using Marketplace.Abstractions;
using Marketplace.Payment;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    internal class AuctionEndedConsumer : IConsumer<AuctionEnded>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILotsRepository _lotsRepository;
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        private readonly ILogger<AuctionEndedConsumer> _logger;
        private readonly ICompensationService _compensationService;

        public AuctionEndedConsumer(
            IOrdersRepository ordersRepository,
            ILotsRepository lotsRepository,
            IPaymentGatewayFactory paymentGatewayFactory,
            ILogger<AuctionEndedConsumer> logger,
            ICompensationService compensationService)
        {
            _ordersRepository = ordersRepository;
            _lotsRepository = lotsRepository;
            _paymentGatewayFactory = paymentGatewayFactory;
            _logger = logger;
            _compensationService = compensationService;
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
            var captureUrl = await _paymentGatewayFactory.CreatePaymentGateway(order.PaymentInfo.Provider)
                .CaptureHoldAsync(order, context.CancellationToken);

            _logger.LogInformation(
                "Hold captured for order {OrderId} (lot {LotId}) via {CaptureUrl}.",
                order.Id, lotId, captureUrl);

            var lot = await _lotsRepository.GetLotById(lotId);

            if (lot is null)
            {
                _logger.LogWarning("Lot {LotId} not found after capture. Skipping stock update.", lotId);
                return;
            }

            lot.Sell(lot.StockCount);
            await _lotsRepository.UpdateLot(lotId, lot);
            await _compensationService.CreateIfNotExists(order.Id, lot.Id, lot.Compensation);

            _logger.LogInformation("Lot {LotId} stock set to 0 after auction capture.", lotId);
        }
    }
}