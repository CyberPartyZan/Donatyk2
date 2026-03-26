namespace Marketplace
{
    public class CheckoutRequest
    {
        public ShippingInfoDto Shipping { get; set; } = new();
        public PaymentInfoDto Payment { get; set; } = new();
    }

    public sealed class CheckoutDrawRequest : CheckoutRequest
    {
        public Guid LotId { get; set; }
        public int TicketsCount { get; set; }
    }

    public sealed class CheckoutAuctionRequest : CheckoutRequest
    {
        public Guid LotId { get; set; }
        public Money Amount { get; set; } = null!;
    }

    public class ShippingInfoDto
    {
        public string RecipientName { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class PaymentInfoDto
    {
        public string Provider { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public string? ReturnUrl { get; set; }
    }
}