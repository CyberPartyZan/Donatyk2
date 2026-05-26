using Microsoft.Extensions.Logging;

namespace Marketplace.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyOrderPaidAsync(Guid orderId, string email)
        {
            _logger.LogInformation("Order {OrderId} paid for {Email}.", orderId, email);
            return Task.CompletedTask;
        }

        public Task NotifyOrderPayFailedAsync(Guid orderId, string email)
        {
            _logger.LogWarning("Order {OrderId} payment failed for {Email}.", orderId, email);
            return Task.CompletedTask;
        }

        public Task NotifyOrderCreatedAsync(Guid orderId, string email)
        {
            _logger.LogInformation("Order {OrderId} created for {Email}.", orderId, email);
            return Task.CompletedTask;
        }

        public Task NotifyShipmentCreatedAsync(Guid orderId, string email, Guid shipmentId, DateTimeOffset createdAt)
        {
            _logger.LogInformation("Shipment {ShipmentId} created for order {OrderId} and {Email} at {CreatedAt}.", shipmentId, orderId, email, createdAt);
            return Task.CompletedTask;
        }

        public Task NotifyPasswordResetAsync(Guid userId, string email, string resetToken)
        {
            _logger.LogInformation("Password reset requested for user {UserId} ({Email}).", userId, email);
            return Task.CompletedTask;
        }

        public Task NotifyEmailConfirmationAsync(Guid userId, string email, string confirmationToken, string? confirmationUrl = null)
        {
            _logger.LogInformation("Email confirmation requested for user {UserId} ({Email}).", userId, email);
            return Task.CompletedTask;
        }

        public Task NotifyDrawWinnerAsync(Guid userId, string email, Guid lotId, Guid winningTicketId)
        {
            _logger.LogInformation("Draw winner user {UserId} ({Email}), lot {LotId}, ticket {TicketId}.", userId, email, lotId, winningTicketId);
            return Task.CompletedTask;
        }
    }
}