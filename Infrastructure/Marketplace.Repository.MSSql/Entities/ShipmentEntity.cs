namespace Marketplace.Repository.MSSql
{
    internal class ShipmentEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ShippingReference { get; set; } = null!;
        public ShipmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual OrderEntity Order { get; set; } = null!;
    }
}