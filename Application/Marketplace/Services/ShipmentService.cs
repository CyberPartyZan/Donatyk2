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

        public async Task<Guid> CreateShipmentAsync(
            Guid orderId,
            string trackingNumber)
        {
            var order = await _ordersRepository.GetById(orderId)
                ?? throw new KeyNotFoundException($"Order '{orderId}' not found.");

            var shipment = Shipment.Create(orderId, trackingNumber, order.ShippingAddress, order.DeliveryCarrier!.Value);
            var shipmentId = await _shipmentRepository.Create(shipment);

            order.AttachShipment(shipmentId);
            await _ordersRepository.Update(order);

            return shipmentId;
        }

        public async Task TakeIntoProcessingAsync(Guid shipmentId)
        {
            var shipment = await GetShipmentAsync(shipmentId);

            shipment.TakeIntoProcessing();

            await _shipmentRepository.Update(shipment);
        }

        public async Task MarkShippedAsync(Guid shipmentId)
        {
            var shipment = await GetShipmentAsync(shipmentId);

            shipment.MarkShipped();

            await _shipmentRepository.Update(shipment);
        }

        public async Task MarkInTransitAsync(Guid shipmentId)
        {
            var shipment = await GetShipmentAsync(shipmentId);

            shipment.MarkInTransit();

            await _shipmentRepository.Update(shipment);
        }

        public async Task MarkOutForDeliveryAsync(Guid shipmentId)
        {
            var shipment = await GetShipmentAsync(shipmentId);

            shipment.MarkOutForDelivery();

            await _shipmentRepository.Update(shipment);
        }

        public async Task MarkDeliveredAsync(Guid shipmentId)
        {
            var shipment = await GetShipmentAsync(shipmentId);

            shipment.MarkDelivered();

            await _shipmentRepository.Update(shipment);

            await _publishEndpoint.Publish(
                new ShipmentDelivered(shipment.OrderId, shipment.Id, shipment.DeliveredAt!.Value));
        }

        private async Task<Shipment> GetShipmentAsync(Guid shipmentId)
        {
            var shipment = await _shipmentRepository.GetById(shipmentId)
                ?? throw new KeyNotFoundException($"Shipment '{shipmentId}' not found.");

            return shipment;
        }
    }
}