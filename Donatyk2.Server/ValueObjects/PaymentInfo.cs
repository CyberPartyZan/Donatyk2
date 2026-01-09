namespace Donatyk2.Server.ValueObjects
{
    public class PaymentInfo
    {
        public string Provider { get; }
        public decimal TaxRate { get; }
        public string? ReturnUrl { get; }
        public string? Reference { get; private set; }

        public PaymentInfo(string provider, decimal taxRate, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentException("Payment provider is required.", nameof(provider));
            }

            if (taxRate < 0 || taxRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(taxRate), "Tax rate must be between 0 and 1.");
            }

            Provider = provider;
            TaxRate = taxRate;
            ReturnUrl = returnUrl;
        }

        public void AttachReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new ArgumentException("Payment reference cannot be empty.", nameof(reference));
            }

            Reference = reference;
        }
    }
}