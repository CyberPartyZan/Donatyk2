using Marketplace.Abstractions;
using Marketplace.Notification;
using MassTransit;

namespace Marketplace
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly INotificationService _notificationService;

        public OrderCreatedConsumer(INotificationService notificationService) {
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var message = context.Message;

            await _notificationService.NotifyOrderCreatedAsync(message.OrderId);

            await Task.CompletedTask;
        }
    }
}
