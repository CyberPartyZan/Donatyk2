namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class PricedItemTests
    {
        [Fact]
        public void FromLot_WithValidInputs_ComputesExpectedValues()
        {
            var lot = CreateLot("Signed poster", 200m, discountedPriceAmount: 170m);
            const int quantity = 2;
            const decimal taxRate = 0.07m;

            var priced = PricedItem.FromLot(lot, quantity, taxRate);

            var currency = lot.Price.Currency;
            Money Round(decimal amount) => new(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency);

            var discountAmount = lot.Price.Amount - lot.DiscountedPrice!.Amount;
            var discountedUnitAmount = lot.DiscountedPrice!.Amount;
            var taxAmount = discountedUnitAmount * taxRate;
            var finalUnitAmount = discountedUnitAmount + taxAmount;

            var expectedDiscount = Round(discountAmount);
            var expectedTax = Round(taxAmount);
            var expectedUnit = Round(finalUnitAmount);
            var expectedTotal = new Money(expectedUnit.Amount * quantity, currency);

            var expectedDiscountPercent = lot.Discount;

            Assert.Equal(lot.Id, priced.LotId);
            Assert.Equal(lot.Name, priced.Name);
            Assert.Equal(lot.Price, priced.BasePrice);
            Assert.Equal(expectedDiscountPercent, priced.DiscountPercent);
            Assert.Equal(taxRate, priced.TaxRate);
            Assert.Equal(quantity, priced.Quantity);
            Assert.Equal(expectedDiscount, priced.DiscountPerUnit);
            Assert.Equal(expectedTax, priced.TaxPerUnit);
            Assert.Equal(expectedUnit, priced.UnitPrice);
            Assert.Equal(expectedTotal, priced.Total);
        }

        [Fact]
        public void FromLot_WithNullDiscountedPrice_UsesBasePrice()
        {
            var lot = CreateLot("Poster", 50m, discountedPriceAmount: null);
            const int quantity = 3;
            const decimal taxRate = 0.2m;

            var priced = PricedItem.FromLot(lot, quantity, taxRate);

            var currency = lot.Price.Currency;
            Money Round(decimal amount) => new(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency);

            var discountAmount = 0m;
            var discountedUnitAmount = lot.Price.Amount;
            var taxAmount = discountedUnitAmount * taxRate;
            var finalUnitAmount = discountedUnitAmount + taxAmount;

            var expectedDiscount = Round(discountAmount);
            var expectedTax = Round(taxAmount);
            var expectedUnit = Round(finalUnitAmount);
            var expectedTotal = new Money(expectedUnit.Amount * quantity, currency);

            Assert.Equal(0d, priced.DiscountPercent);
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

        private static Lot CreateLot(string name, decimal priceAmount, decimal? discountedPriceAmount = null, Currency currency = Currency.USD)
        {
            var price = new Money(priceAmount, currency);
            var compensation = new Money(Math.Max(priceAmount - 20m, 0m), currency);
            Money? discountedPrice = discountedPriceAmount is null ? null : new Money(discountedPriceAmount.Value, currency);

            return new Lot(
                Guid.NewGuid(),
                name,
                $"{name} description",
                price,
                compensation,
                stockCount: 5,
                discountedPrice,
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