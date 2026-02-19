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

        public Task NotifyOrderPaidAsync(Guid userId, Guid orderId)
        {
            _logger.LogInformation("Order {OrderId} for user {UserId} has been marked as paid.", orderId, userId);
            return Task.CompletedTask;
        }

        public Task NotifyPasswordResetAsync(Guid userId, string email, string resetToken)
        {
            // TODO: Implement SendGrid or SMTP email sending here.
            var preview = resetToken.Length <= 8 ? resetToken : $"{resetToken[..8]}...";
            _logger.LogInformation(
                "Password reset requested for user {UserId} ({Email}). Token preview: {TokenPreview}",
                userId,
                email,
                preview);

            return Task.CompletedTask;
        }

        public Task NotifyEmailConfirmationAsync(
            Guid userId,
            string email,
            string confirmationToken,
            string? confirmationUrl = null)
        {
            var preview = confirmationToken.Length <= 8 ? confirmationToken : $"{confirmationToken[..8]}...";
            if (!string.IsNullOrWhiteSpace(confirmationUrl))
            {
                _logger.LogInformation(
                    "Email confirmation requested for user {UserId} ({Email}). Token preview: {TokenPreview}. Confirmation URL: {ConfirmationUrl}",
                    userId,
                    email,
                    preview,
                    confirmationUrl);
            }
            else
            {
                _logger.LogInformation(
                    "Email confirmation requested for user {UserId} ({Email}). Token preview: {TokenPreview}",
                    userId,
                    email,
                    preview);
            }

            return Task.CompletedTask;
        }
    }
}