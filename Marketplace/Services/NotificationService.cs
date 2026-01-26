using Donatyk2.Server.Services.Interfaces;

namespace Donatyk2.Server.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyOrderPaidAsync(Guid userId, Guid orderId)
        {
            _logger.LogInformation("Order {OrderId} for user {UserId} has been marked as paid.", orderId, userId);
            return Task.CompletedTask;
        }
    }
}