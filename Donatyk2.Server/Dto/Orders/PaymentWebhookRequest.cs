namespace Donatyk2.Server.Dto.Orders
{
    public class PaymentWebhookRequest
    {
        public Guid OrderId { get; set; }
        // TODO: Provider should be enum
        public string Provider { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}