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

        public async Task NotifyOrderPaidAsync(Guid orderId, string email)
        {
            var subject = "Your order payment was successful";
            var plainText = $"Order {orderId} has been marked as paid.";
            var html = $"""
                        <p>Your payment was successful.</p>
                        <p><strong>Order ID:</strong> {orderId}</p>
                        """;

            await SendEmailAsync(email, subject, plainText, html);
            _logger.LogInformation("Order paid email queued for order {OrderId} to {Email}.", orderId, email);
        }

        public async Task NotifyOrderPayFailedAsync(Guid orderId, string email)
        {
            var subject = "Your order payment failed";
            var plainText = $"Payment for order {orderId} failed.";
            var html = $"""
                        <p>Payment for your order failed.</p>
                        <p><strong>Order ID:</strong> {orderId}</p>
                        """;

            await SendEmailAsync(email, subject, plainText, html);
            _logger.LogWarning("Order payment failed email queued for order {OrderId} to {Email}.", orderId, email);
        }

        public async Task NotifyOrderCreatedAsync(Guid orderId, string email)
        {
            var subject = "Your order was created";
            var plainText = $"Order {orderId} has been created.";
            var html = $"""
                        <p>Your order has been created.</p>
                        <p><strong>Order ID:</strong> {orderId}</p>
                        """;

            await SendEmailAsync(email, subject, plainText, html);
            _logger.LogInformation("Order created email queued for order {OrderId} to {Email}.", orderId, email);
        }

        public async Task NotifyShipmentCreatedAsync(Guid orderId, string email, Guid shipmentId, DateTimeOffset createdAt)
        {
            var subject = "Your shipment was created";
            var plainText = $"Shipment {shipmentId} for order {orderId} was created at {createdAt:u}.";
            var html = $"""
                        <p>Your shipment has been created.</p>
                        <p><strong>Order ID:</strong> {orderId}</p>
                        <p><strong>Shipment ID:</strong> {shipmentId}</p>
                        <p><strong>Created:</strong> {createdAt:u}</p>
                        """;

            await SendEmailAsync(email, subject, plainText, html);
            _logger.LogInformation("Shipment created email queued for order {OrderId} to {Email}.", orderId, email);
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

        public async Task NotifyEmailConfirmationAsync(Guid userId, string email, string confirmationToken, string? confirmationUrl = null)
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

        public async Task NotifyDrawWinnerAsync(Guid userId, string email, Guid lotId, Guid winningTicketId)
        {
            var subject = "You won the draw";
            var plainText = $"You won lot {lotId} with ticket {winningTicketId}.";
            var html = $"""
                        <p>Congratulations, you won the draw!</p>
                        <p><strong>Lot ID:</strong> {lotId}</p>
                        <p><strong>Winning Ticket ID:</strong> {winningTicketId}</p>
                        """;

            await SendEmailAsync(email, subject, plainText, html);
            _logger.LogInformation("Draw winner email queued for user {UserId} ({Email}) on lot {LotId}.", userId, email, lotId);
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