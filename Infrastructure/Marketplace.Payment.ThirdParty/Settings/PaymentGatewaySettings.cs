namespace Marketplace.Payment
{
    public class PaymentGatewaySettings
    {
        public const string SectionName = "Payments";

        /// <summary>
        /// Which gateway to activate: "Stripe" or "Fake"
        /// </summary>
        public string Provider { get; set; } = "Fake";
        public string BaseUrl { get; set; } = "https://payments.local";
    }
}