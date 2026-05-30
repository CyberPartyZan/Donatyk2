namespace Marketplace.Repository.MSSql
{
    internal class ShippingAddressEntity
    {
        public Guid Id { get; set; }
        public string RecipientName { get; set; } = null!;
        public string Line1 { get; set; } = null!;
        public string? Line2 { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public virtual OrderEntity? Order { get; set; }
        public virtual ShipmentEntity? Shipment { get; set; }
    }
}