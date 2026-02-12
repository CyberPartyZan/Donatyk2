using System;
using Donatyk2.Server.ValueObjects;
using Xunit;

namespace Marketplace.Abstractions.Unit.Tests.ValueObjects
{
    public sealed class PaymentInfoTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsProperties()
        {
            const string provider = "Stripe";
            const decimal taxRate = 0.25m;
            const string returnUrl = "https://example.com/return";

            var paymentInfo = new PaymentInfo(provider, taxRate, returnUrl);

            Assert.Equal(provider, paymentInfo.Provider);
            Assert.Equal(taxRate, paymentInfo.TaxRate);
            Assert.Equal(returnUrl, paymentInfo.ReturnUrl);
            Assert.Null(paymentInfo.Reference);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidProvider_ThrowsArgumentException(string? provider)
        {
            Assert.Throws<ArgumentException>(() => new PaymentInfo(provider!, 0.1m, "https://return"));
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(1.01)]
        public void Constructor_WithTaxRateOutsideRange_ThrowsArgumentOutOfRangeException(double taxRate)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PaymentInfo("Stripe", (decimal)taxRate, null));
        }

        [Fact]
        public void AttachReference_WithValidReference_SetsReference()
        {
            var paymentInfo = new PaymentInfo("Stripe", 0.2m, null);

            paymentInfo.AttachReference("REF-123");

            Assert.Equal("REF-123", paymentInfo.Reference);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void AttachReference_WithInvalidReference_ThrowsArgumentException(string? reference)
        {
            var paymentInfo = new PaymentInfo("Stripe", 0.2m, null);

            Assert.Throws<ArgumentException>(() => paymentInfo.AttachReference(reference!));
        }
    }
}
