namespace Marketplace.Payment
{
    public class StripeSettings
    {
        public const string SectionName = "Stripe";

        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string Provider { get; set; } = "Stripe";
    }
}