using Marketplace.Abstractions;
using Marketplace.Notification;
using MassTransit;

namespace Marketplace
{
    public class PaymentProcessedConsumer : IConsumer<PaymentProcessed>
    {
        private readonly INotificationService _notificationService;

        public PaymentProcessedConsumer(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<PaymentProcessed> context)
        {
            var message = context.Message;

            if (!message.Succeeded) 
            {
                await _notificationService.NotifyOrderPayFailedAsync(message.OrderId);
                return;
            }

            await _notificationService.NotifyOrderPaidAsync(message.OrderId);

            // Handle the event
            return;
        }
    }
}
