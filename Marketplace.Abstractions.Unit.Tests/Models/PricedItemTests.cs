namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class PricedItemTests
    {
        [Fact]
        public void FromLot_WithValidInputs_ComputesExpectedValues()
        {
            var lot = CreateLot("Signed poster", 200m, discount: 15);
            const int quantity = 2;
            const decimal taxRate = 0.07m;

            var priced = PricedItem.FromLot(lot, quantity, taxRate);

            var currency = lot.Price.Currency;
            Money Round(decimal amount) => new(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency);

            var discountAmount = lot.Price.Amount * (decimal)(lot.Discount / 100d);
            var discountedUnitAmount = lot.Price.Amount - discountAmount;
            var taxAmount = discountedUnitAmount * taxRate;
            var finalUnitAmount = discountedUnitAmount + taxAmount;

            var expectedDiscount = Round(discountAmount);
            var expectedTax = Round(taxAmount);
            var expectedUnit = Round(finalUnitAmount);
            var expectedTotal = new Money(expectedUnit.Amount * quantity, currency);

            Assert.Equal(lot.Id, priced.LotId);
            Assert.Equal(lot.Name, priced.Name);
            Assert.Equal(lot.Price, priced.BasePrice);
            Assert.Equal(lot.Discount, priced.DiscountPercent);
            Assert.Equal(taxRate, priced.TaxRate);
            Assert.Equal(quantity, priced.Quantity);
            Assert.Equal(expectedDiscount, priced.DiscountPerUnit);
            Assert.Equal(expectedTax, priced.TaxPerUnit);
            Assert.Equal(expectedUnit, priced.UnitPrice);
            Assert.Equal(expectedTotal, priced.Total);
        }

        [Fact]
        public void FromLot_WithNullLot_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PricedItem.FromLot(null!, 1, 0.1m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void FromLot_WithNonPositiveQuantity_ThrowsArgumentOutOfRangeException(int quantity)
        {
            var lot = CreateLot("Poster", 50m);

            Assert.Throws<ArgumentOutOfRangeException>(() => PricedItem.FromLot(lot, quantity, 0.1m));
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(1.01)]
        public void FromLot_WithInvalidTaxRate_ThrowsArgumentOutOfRangeException(double taxRate)
        {
            var lot = CreateLot("Poster", 50m);

            Assert.Throws<ArgumentOutOfRangeException>(() => PricedItem.FromLot(lot, 1, (decimal)taxRate));
        }

        private static Lot CreateLot(string name, decimal priceAmount, double discount = 0, Currency currency = Currency.USD)
        {
            var price = new Money(priceAmount, currency);
            var compensation = new Money(Math.Max(priceAmount - 20m, 0m), currency);

            return new Lot(
                Guid.NewGuid(),
                name,
                $"{name} description",
                price,
                compensation,
                stockCount: 5,
                discount: discount,
                LotType.Simple,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false);
        }

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Valid seller", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}