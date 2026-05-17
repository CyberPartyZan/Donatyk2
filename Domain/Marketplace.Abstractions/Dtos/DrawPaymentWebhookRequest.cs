namespace Marketplace
{
    public class DrawPaymentWebhookRequest
    {
        public Guid OrderId { get; set; }
        public Guid LotId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}