namespace Marketplace.Repository.MSSql
{
    internal class OrderEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SellerId { get; set; }
        public virtual SellerEntity Seller { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public Money Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ShippingAddressId { get; set; }
        public string PaymentProvider { get; set; } = null!;
        public decimal PaymentTaxRate { get; set; }
        public string? PaymentReturnUrl { get; set; }
        public string? PaymentReference { get; set; }
        public Guid? ShipmentId { get; set; }
        public DeliveryCarrier? DeliveryCarrier { get; set; }
        public virtual ShippingAddressEntity ShippingAddress { get; set; } = null!;
        public virtual ShipmentEntity? Shipment { get; set; }
        public virtual ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
    }
}
