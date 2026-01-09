namespace Donatyk2.Server.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotifyOrderPaidAsync(Guid userId, Guid orderId);
    }
}