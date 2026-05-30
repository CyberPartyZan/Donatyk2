namespace Marketplace
{
    public class Shipment
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public string TrackingNumber { get; private set; } = null!;
        public ShipmentStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Shipment() { }

        public static Shipment Create(Guid orderId, string shippingReference)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("OrderId must be provided.", nameof(orderId));

            if (string.IsNullOrWhiteSpace(shippingReference))
                throw new ArgumentException("ShippingReference must be provided.", nameof(shippingReference));

            return new Shipment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                TrackingNumber = shippingReference,
                Status = ShipmentStatus.Created,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Shipment Reconstruct(
            Guid id,
            Guid orderId,
            string shippingReference,
            ShipmentStatus status,
            DateTime createdAt)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be provided.", nameof(id));

            if (orderId == Guid.Empty)
                throw new ArgumentException("OrderId must be provided.", nameof(orderId));

            if (string.IsNullOrWhiteSpace(shippingReference))
                throw new ArgumentException("ShippingReference must be provided.", nameof(shippingReference));

            return new Shipment
            {
                Id = id,
                OrderId = orderId,
                TrackingNumber = shippingReference,
                Status = status,
                CreatedAt = createdAt
            };
        }

        public void TakeIntoProcessing()
        {
            if (Status != ShipmentStatus.Created)
                throw new InvalidOperationException("Only created shipments can be taken into processing.");

            Status = ShipmentStatus.Processing;
        }

        public void MarkShipped()
        {
            if (Status != ShipmentStatus.Processing)
                throw new InvalidOperationException("Only processing shipments can be marked as shipped.");

            Status = ShipmentStatus.Shipped;
        }

        public void MarkInTransit()
        {
            if (Status != ShipmentStatus.Shipped)
                throw new InvalidOperationException("Only shipped shipments can be marked as in transit.");

            Status = ShipmentStatus.InTransit;
        }

        public void MarkOutForDelivery()
        {
            if (Status != ShipmentStatus.InTransit)
                throw new InvalidOperationException("Only in-transit shipments can be marked as out for delivery.");

            Status = ShipmentStatus.OutForDelivery;
        }

        public void MarkDelivered()
        {
            if (Status != ShipmentStatus.OutForDelivery)
                throw new InvalidOperationException("Only out-for-delivery shipments can be marked as delivered.");

            Status = ShipmentStatus.Delivered;
        }
    }
}