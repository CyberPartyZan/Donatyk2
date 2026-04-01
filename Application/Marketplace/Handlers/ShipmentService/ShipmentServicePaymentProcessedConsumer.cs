using Marketplace.Abstractions;
using MassTransit;

namespace Marketplace
{
    internal class ShipmentServicePaymentProcessedConsumer : IConsumer<PaymentProcessed>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public ShipmentServicePaymentProcessedConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<PaymentProcessed> context)
        {
            var message = context.Message;

            if (!message.Succeeded)
            {
                return;
            }

            // TODO: Add shipment service and create shipment for the order here
            var shipmentCreated = Guid.NewGuid(); // Simulate shipment creation

            await _publishEndpoint.Publish(new ShipmentCreated (message.OrderId, shipmentCreated, DateTime.UtcNow));
        }
    }
}
