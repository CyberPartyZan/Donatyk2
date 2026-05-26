using Marketplace.Abstractions;
using Marketplace.Notification;
using Marketplace.Repository;
using MassTransit;

namespace Marketplace
{
    internal class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
    {
        private readonly INotificationService _notificationService;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IUsersRepository _usersRepository;

        public ShipmentCreatedConsumer(
            INotificationService notificationService,
            IOrdersRepository ordersRepository,
            IUsersRepository usersRepository)
        {
            _notificationService = notificationService;
            _ordersRepository = ordersRepository;
            _usersRepository = usersRepository;
        }

        public async Task Consume(ConsumeContext<ShipmentCreated> context)
        {
            var message = context.Message;

            var order = await _ordersRepository.GetById(message.OrderId)
                ?? throw new KeyNotFoundException($"Order '{message.OrderId}' not found.");

            var user = await _usersRepository.GetById(order.CustomerId)
                ?? throw new KeyNotFoundException($"User '{order.CustomerId}' not found.");

            await _notificationService.NotifyShipmentCreatedAsync(message.OrderId, user.Email, message.ShipmentId, message.CreatedAt);
        }
    }
}
