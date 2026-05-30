using Marketplace.Abstractions;
using MassTransit;

namespace Marketplace
{
    internal class ShipmentServicePaymentProcessedConsumer : IConsumer<PaymentProcessed>
    {
        private readonly IShipmentService _shipmentService;
        private readonly IPublishEndpoint _publishEndpoint;

        public ShipmentServicePaymentProcessedConsumer(
            IShipmentService shipmentService,
            IPublishEndpoint publishEndpoint)
        {
            _shipmentService = shipmentService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<PaymentProcessed> context)
        {
            var message = context.Message;

            if (!message.Succeeded)
                return;

            // TODO: Derive a real tracking reference from the payment/logistics provider
            var trackingNumber = $"SHIP-{message.OrderId:N}";

            var shipmentId = await _shipmentService.CreateShipmentAsync(
                message.OrderId,
                trackingNumber);

            await _publishEndpoint.Publish(new ShipmentCreated(message.OrderId, shipmentId, DateTime.UtcNow));
        }
    }
}
