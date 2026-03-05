namespace Marketplace.Notification
{
    public interface INotificationService
    {
        Task NotifyOrderPaidAsync(Guid orderId);
        Task NotifyOrderCreatedAsync(Guid orderId);
        Task NotifyPasswordResetAsync(Guid userId, string email, string resetToken);
        Task NotifyEmailConfirmationAsync(
            Guid userId,
            string email,
            string confirmationToken,
            string? confirmationUrl = null);
    }
}