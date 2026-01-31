namespace Donatyk2.Server.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotifyOrderPaidAsync(Guid userId, Guid orderId);
        Task NotifyPasswordResetAsync(Guid userId, string email, string resetToken);
        Task NotifyEmailConfirmationAsync(
            Guid userId,
            string email,
            string confirmationToken,
            string? confirmationUrl = null);
    }
}