using Marketplace.Abstractions;
using Marketplace.Repository;
using MassTransit;

namespace Marketplace
{
    internal class ShipmentDeliveredConsumer : IConsumer<ShipmentDelivered>
    {
        private readonly IOrdersRepository _ordersRepository;

        public ShipmentDeliveredConsumer(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        public async Task Consume(ConsumeContext<ShipmentDelivered> context)
        {
            var message = context.Message;

            var order = await _ordersRepository.GetById(message.OrderId)
                ?? throw new KeyNotFoundException($"Order '{message.OrderId}' not found.");

            order.MarkCompleted();

            await _ordersRepository.Update(order);
        }
    }
}