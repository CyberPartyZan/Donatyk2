using Donatyk2.Server.Enums;
using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Data
{
    internal class OrderEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public Money Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ShippingRecipientName { get; set; } = null!;
        public string ShippingLine1 { get; set; } = null!;
        public string? ShippingLine2 { get; set; }
        public string ShippingCity { get; set; } = null!;
        public string ShippingState { get; set; } = null!;
        public string ShippingPostalCode { get; set; } = null!;
        public string ShippingCountry { get; set; } = null!;
        public string ShippingPhone { get; set; } = null!;
        public string PaymentProvider { get; set; } = null!;
        public decimal PaymentTaxRate { get; set; }
        public string? PaymentReturnUrl { get; set; }
        public string? PaymentReference { get; set; }
        public virtual ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
    }
}
