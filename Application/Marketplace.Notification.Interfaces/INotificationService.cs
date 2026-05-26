namespace Marketplace.Notification
{
    public interface INotificationService
    {
        Task NotifyOrderPaidAsync(Guid orderId, string email);
        Task NotifyOrderPayFailedAsync(Guid orderId, string email);
        Task NotifyOrderCreatedAsync(Guid orderId, string email);
        Task NotifyShipmentCreatedAsync(Guid orderId, string email, Guid shipmentId, DateTimeOffset createdAt);
        Task NotifyPasswordResetAsync(Guid userId, string email, string resetToken);
        Task NotifyEmailConfirmationAsync(
            Guid userId,
            string email,
            string confirmationToken,
            string? confirmationUrl = null);
        Task NotifyDrawWinnerAsync(Guid userId, string email, Guid lotId, Guid winningTicketId);
    }
}