using Marketplace.Abstractions;
using Marketplace.Repository;
using MassTransit;

namespace Marketplace
{
    internal class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ShipmentService(
            IShipmentRepository shipmentRepository,
            IOrdersRepository ordersRepository,
            IPublishEndpoint publishEndpoint)
        {
            _shipmentRepository = shipmentRepository;
            _ordersRepository = ordersRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> CreateShipmentAsync(Guid orderId, string shippingReference)
        {
            var order = await _ordersRepository.GetById(orderId)
                ?? throw new KeyNotFoundException($"Order '{orderId}' not found.");

            var shipment = Shipment.Create(orderId, shippingReference);
            var shipmentId = await _shipmentRepository.Create(shipment);

            order.AttachShipment(shipmentId);
            await _ordersRepository.Update(order);

            return shipmentId;
        }

        public async Task TakeIntoProcessingAsync(Guid shipmentId)
        {
            var (shipment, order) = await GetShipmentAndOrderAsync(shipmentId);

            shipment.TakeIntoProcessing();
            order.MarkProcessing();

            await _shipmentRepository.Update(shipment);
            await _ordersRepository.Update(order);
        }

        public async Task MarkShippedAsync(Guid shipmentId)
        {
            var (shipment, order) = await GetShipmentAndOrderAsync(shipmentId);

            shipment.MarkShipped();
            order.MarkShipped();

            await _shipmentRepository.Update(shipment);
            await _ordersRepository.Update(order);
        }

        public async Task MarkInTransitAsync(Guid shipmentId)
        {
            var (shipment, order) = await GetShipmentAndOrderAsync(shipmentId);

            shipment.MarkInTransit();
            order.MarkInTransit();

            await _shipmentRepository.Update(shipment);
            await _ordersRepository.Update(order);
        }

        public async Task MarkOutForDeliveryAsync(Guid shipmentId)
        {
            var (shipment, order) = await GetShipmentAndOrderAsync(shipmentId);

            shipment.MarkOutForDelivery();
            order.MarkOutForDelivery();

            await _shipmentRepository.Update(shipment);
            await _ordersRepository.Update(order);
        }

        public async Task MarkDeliveredAsync(Guid shipmentId)
        {
            var (shipment, order) = await GetShipmentAndOrderAsync(shipmentId);

            shipment.MarkDelivered();
            order.MarkDelivered();

            await _shipmentRepository.Update(shipment);
            await _ordersRepository.Update(order);

            await _publishEndpoint.Publish(
                new ShipmentDelivered(order.Id, shipment.Id, shipment.DeliveredAt!.Value));
        }

        private async Task<(Shipment shipment, Order order)> GetShipmentAndOrderAsync(Guid shipmentId)
        {
            var shipment = await _shipmentRepository.GetById(shipmentId)
                ?? throw new KeyNotFoundException($"Shipment '{shipmentId}' not found.");

            var order = await _ordersRepository.GetById(shipment.OrderId)
                ?? throw new KeyNotFoundException($"Order '{shipment.OrderId}' not found.");

            return (shipment, order);
        }
    }
}