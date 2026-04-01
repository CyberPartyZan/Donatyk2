namespace Marketplace.Abstractions.Unit.Tests.ValueObjects
{
    public sealed class ShippingInfoTests
    {
        private const string Recipient = "Alice Doe";
        private const string Line1 = "123 Main St";
        private const string City = "Kyiv";
        private const string State = "Kyiv";
        private const string PostalCode = "01001";
        private const string Country = "Ukraine";
        private const string Phone = "+380441234567";

        [Fact]
        public void Constructor_WithValidArguments_SetsAllProperties()
        {
            const string line2 = "Suite 500";

            var shipping = CreateShippingInfo(line2);

            Assert.Equal(Recipient, shipping.RecipientName);
            Assert.Equal(Line1, shipping.Line1);
            Assert.Equal(line2, shipping.Line2);
            Assert.Equal(City, shipping.City);
            Assert.Equal(State, shipping.State);
            Assert.Equal(PostalCode, shipping.PostalCode);
            Assert.Equal(Country, shipping.Country);
            Assert.Equal(Phone, shipping.Phone);
        }

        [Fact]
        public void Constructor_WithNullLine2_AllowsNullSecondaryLine()
        {
            var shipping = CreateShippingInfo(line2: null);

            Assert.Null(shipping.Line2);
        }

        [Theory]
        [MemberData(nameof(InvalidRequiredFieldCases))]
        public void Constructor_WithInvalidRequiredField_ThrowsArgumentException(Action construct, string expectedParamName)
        {
            var exception = Assert.Throws<ArgumentException>(construct);
            Assert.Equal(expectedParamName, exception.ParamName);
        }

        public static IEnumerable<object[]> InvalidRequiredFieldCases()
        {
            yield return new object[]
            {
                (Action)(() => new ShippingInfo(
                    recipientName: null!,
                    line1: Line1,
                    line2: null,
                    city: City,
                    state: State,
                    postalCode: PostalCode,
                    country: Country,
                    phone: Phone)),
                "recipientName"
            };

            yield return new object[]
            {
                (Action)(() => new ShippingInfo(
                    recipientName: Recipient,
                    line1: "   ",
                    line2: null,
                    city: City,
                    state: State,
                    postalCode: PostalCode,
                    country: Country,
                    phone: Phone)),
                "line1"
            };

            yield return new object[]
            {
                (Action)(() => new ShippingInfo(
                    recipientName: Recipient,
                    line1: Line1,
                    line2: null,
                    city: null!,
                    state: State,
                    postalCode: PostalCode,
                    country: Country,
                    phone: Phone)),
                "city"
            };

            yield return new object[]
            {
                (Action)(() => new ShippingInfo(
                    recipientName: Recipient,
                    line1: Line1,
                    line2: null,
                    city: City,
                    state: "",
                    postalCode: PostalCode,
                    country: Country,
                    phone: Phone)),
                "state"
            };

            yield return new object[]
            {
                (Action)(() => new ShippingInfo(
                    recipientName: Recipient,
                    line1: Line1,
                    line2: null,
                    city: City,
                    state: State,
                    postalCode: null!,
                    country: Country,
                    phone: Phone)),
                "postalCode"
            };

            yield return new object[]
            {
                (Action)(() => new ShippingInfo(
                    recipientName: Recipient,
                    line1: Line1,
                    line2: null,
                    city: City,
                    state: State,
                    postalCode: PostalCode,
                    country: " ",
                    phone: Phone)),
                "country"
            };

            yield return new object[]
            {
                (Action)(() => new ShippingInfo(
                    recipientName: Recipient,
                    line1: Line1,
                    line2: null,
                    city: City,
                    state: State,
                    postalCode: PostalCode,
                    country: Country,
                    phone: null!)),
                "phone"
            };
        }

        private static ShippingInfo CreateShippingInfo(string? line2) =>
            new(Recipient, Line1, line2, City, State, PostalCode, Country, Phone);
    }
}
