namespace Marketplace.Repository.MSSql
{
    internal class ShipmentEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ShippingReference { get; set; } = null!;
        public ShipmentStatus Status { get; set; }
        public DeliveryCarrier Carrier { get; set; }
        public Guid ShippingAddressId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public virtual ShippingAddressEntity ShippingAddress { get; set; } = null!;
        public virtual OrderEntity Order { get; set; } = null!;
    }
}