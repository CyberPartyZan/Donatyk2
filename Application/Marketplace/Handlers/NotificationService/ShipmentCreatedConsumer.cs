using Marketplace.Abstractions;
using Marketplace.Notification;
using MassTransit;

namespace Marketplace
{
    internal class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
    {
        private readonly INotificationService _notificationService;

        public ShipmentCreatedConsumer(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<ShipmentCreated> context)
        {
            var message = context.Message;

            await _notificationService.NotifyShipmentCreatedAsync(message.OrderId, message.ShipmentId, message.CreatedAt);
        }
    }
}
