using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Marketplace.Notification
{
    public class SendGridNotificationService : INotificationService
    {
        private readonly ILogger<SendGridNotificationService> _logger;
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridOptions _options;

        public SendGridNotificationService(
            ILogger<SendGridNotificationService> logger,
            ISendGridClient sendGridClient,
            IOptions<SendGridOptions> options)
        {
            _logger = logger;
            _sendGridClient = sendGridClient;
            _options = options.Value;
        }

        public Task NotifyOrderPaidAsync(Guid orderId)
        {
            _logger.LogInformation("Order {OrderId} has been marked as paid.", orderId);
            return Task.CompletedTask;
        }

        public Task NotifyOrderPayFailedAsync(Guid orderId)
        {
            _logger.LogWarning("Payment failed for order {OrderId}.", orderId);
            return Task.CompletedTask;
        }

        public Task NotifyOrderCreatedAsync(Guid orderId)
        {
            _logger.LogInformation("Order {OrderId} has been created.", orderId);
            return Task.CompletedTask;
        }

        public Task NotifyShipmentCreatedAsync(Guid orderId, Guid shipmentId, DateTimeOffset createdAt)
        {
            _logger.LogInformation(
                "Shipment {ShipmentId} created for order {OrderId} at {CreatedAt}.",
                shipmentId,
                orderId,
                createdAt);

            return Task.CompletedTask;
        }

        public async Task NotifyPasswordResetAsync(Guid userId, string email, string resetToken)
        {
            var subject = "Reset your password";
            var plainText = $"Password reset was requested for your account. Reset token: {resetToken}";
            var html = $"""
                        <p>Password reset was requested for your account.</p>
                        <p><strong>Reset token:</strong> {resetToken}</p>
                        """;

            await SendEmailAsync(email, subject, plainText, html);

            _logger.LogInformation("Password reset email queued for user {UserId} ({Email}).", userId, email);
        }

        public async Task NotifyEmailConfirmationAsync(
            Guid userId,
            string email,
            string confirmationToken,
            string? confirmationUrl = null)
        {
            var subject = "Confirm your email";
            var plainText = string.IsNullOrWhiteSpace(confirmationUrl)
                ? $"Please confirm your email with this token: {confirmationToken}"
                : $"Please confirm your email using this link: {confirmationUrl}";

            var html = string.IsNullOrWhiteSpace(confirmationUrl)
                ? $"""
                   <p>Please confirm your email with this token:</p>
                   <p><strong>{confirmationToken}</strong></p>
                   """
                : $"""
                   <p>Please confirm your email by clicking the link below:</p>
                   <p><a href="{confirmationUrl}">Confirm email</a></p>
                   """;

            await SendEmailAsync(email, subject, plainText, html);

            _logger.LogInformation("Email confirmation email queued for user {UserId} ({Email}).", userId, email);
        }

        public Task NotifyDrawWinnerAsync(Guid userId, Guid lotId, Guid winningTicketId)
        {
            _logger.LogInformation(
                "User {UserId} has won the draw for lot {LotId} with ticket {WinningTicketId}.",
                userId,
                lotId,
                winningTicketId);

            return Task.CompletedTask;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Skipping email send because recipient email is empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                _logger.LogWarning("Skipping email send because SendGrid configuration is incomplete.");
                return;
            }

            var message = MailHelper.CreateSingleEmail(
                new EmailAddress(_options.FromEmail, _options.FromName),
                new EmailAddress(toEmail),
                subject,
                plainTextContent,
                htmlContent);

            var response = await _sendGridClient.SendEmailAsync(message);

            if ((int)response.StatusCode >= 400)
            {
                _logger.LogWarning(
                    "SendGrid returned non-success status code {StatusCode} for recipient {Recipient}.",
                    response.StatusCode,
                    toEmail);
            }
        }
    }
}