namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class OrderItemTests
    {
        [Fact]
        public void From_WithValidPricedItem_PopulatesOrderItem()
        {
            var priced = PricedItem.FromLot(CreateLot("Special lot", 150m, discount: 10), quantity: 3, taxRate: 0.05m);

            var orderItem = OrderItem.From(priced);

            Assert.Equal(priced.LotId, orderItem.LotId);
            Assert.Equal(priced.Name, orderItem.NameSnapshot);
            Assert.Equal(priced.UnitPrice, orderItem.UnitPrice);
            Assert.Equal(priced.Quantity, orderItem.Quantity);
            Assert.Equal(priced.Total, orderItem.Total);
        }

        [Fact]
        public void From_WithNullPricedItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => OrderItem.From(null!));
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