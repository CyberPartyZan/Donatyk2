namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class CartItemTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsProperties()
        {
            var lot = CreateLot();
            var userId = Guid.NewGuid();

            var item = new CartItem(lot, quantity: 2, userId);

            Assert.Equal(lot, item.Lot);
            Assert.Equal(2, item.Quantity);
            Assert.Equal(userId, item.UserId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Constructor_WithNonPositiveQuantity_ThrowsArgumentOutOfRangeException(int quantity)
        {
            var lot = CreateLot();
            var userId = Guid.NewGuid();

            Assert.Throws<ArgumentOutOfRangeException>(() => new CartItem(lot, quantity, userId));
        }

        [Fact]
        public void Constructor_WithEmptyUserId_ThrowsArgumentException()
        {
            var lot = CreateLot();

            Assert.Throws<ArgumentException>(() => new CartItem(lot, quantity: 1, Guid.Empty));
        }

        [Fact]
        public void Constructor_WithNullLot_ThrowsArgumentNullException()
        {
            var userId = Guid.NewGuid();

            Assert.Throws<ArgumentNullException>(() => new CartItem(null!, quantity: 1, userId));
        }

        private static Lot CreateLot() =>
            new(
                Guid.NewGuid(),
                "Test lot",
                "Lot description",
                new Money(100m, Currency.USD),
                new Money(80m, Currency.USD),
                stockCount: 1,
                discount: 0,
                LotType.Simple,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false);

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}
