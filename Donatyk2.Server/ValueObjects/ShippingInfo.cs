namespace Donatyk2.Server.ValueObjects
{
    public class ShippingInfo
    {
        public string RecipientName { get; }
        public string Line1 { get; }
        public string? Line2 { get; }
        public string City { get; }
        public string State { get; }
        public string PostalCode { get; }
        public string Country { get; }
        public string Phone { get; }

        public ShippingInfo(
            string recipientName,
            string line1,
            string? line2,
            string city,
            string state,
            string postalCode,
            string country,
            string phone)
        {
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                throw new ArgumentException("Recipient name is required.", nameof(recipientName));
            }

            if (string.IsNullOrWhiteSpace(line1))
            {
                throw new ArgumentException("Address line 1 is required.", nameof(line1));
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("City is required.", nameof(city));
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException("State/Region is required.", nameof(state));
            }

            if (string.IsNullOrWhiteSpace(postalCode))
            {
                throw new ArgumentException("Postal code is required.", nameof(postalCode));
            }

            if (string.IsNullOrWhiteSpace(country))
            {
                throw new ArgumentException("Country is required.", nameof(country));
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("Phone is required.", nameof(phone));
            }

            RecipientName = recipientName;
            Line1 = line1;
            Line2 = line2;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
            Phone = phone;
        }
    }
}