namespace Marketplace
{
    public sealed class ShipmentAdminDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string LotName { get; set; } = null!;
        public string LotImage { get; set; } = string.Empty;
        public string BuyerName { get; set; } = null!;
        public string Carrier { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string RecipientPhone { get; set; } = null!;
        public ShipmentStatus Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}