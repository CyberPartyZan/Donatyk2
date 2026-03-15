namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class OrderTests
    {
        [Fact]
        public void Create_WithValidArguments_BuildsOrderWithExpectedTotals()
        {
            var before = DateTime.UtcNow;
            var userId = Guid.NewGuid();
            var shipping = CreateShippingInfo();
            var payment = CreatePaymentInfo();
            var items = new[]
            {
                PricedItem.FromLot(CreateLot("Lot A", 120m), quantity: 2, taxRate: 0.05m),
                PricedItem.FromLot(CreateLot("Lot B", 80m), quantity: 1, taxRate: 0.05m)
            };

            var order = Order.Create(userId, shipping, payment, items);
            var after = DateTime.UtcNow;

            var expectedAmount = items.Sum(i => i.Total.Amount);
            var expectedTotal = new Money(expectedAmount, items[0].Total.Currency);

            Assert.Equal(userId, order.CustomerId);
            Assert.Equal(OrderStatus.Created, order.Status);
            Assert.Equal(shipping, order.ShippingInfo);
            Assert.Equal(payment, order.PaymentInfo);
            Assert.Equal(expectedTotal, order.Total);
            Assert.Equal(items.Length, order.Items.Count);
            Assert.InRange(order.CreatedAt, before, after);
        }

        [Fact]
        public void Create_WithEmptyUserId_ThrowsArgumentException()
        {
            var shipping = CreateShippingInfo();
            var payment = CreatePaymentInfo();
            var items = new[] { PricedItem.FromLot(CreateLot("Lot", 50m), 1, 0.1m) };

            Assert.Throws<ArgumentException>(() => Order.Create(Guid.Empty, shipping, payment, items));
        }

        [Fact]
        public void Create_WithNullShippingInfo_ThrowsArgumentNullException()
        {
            var payment = CreatePaymentInfo();
            var items = new[] { PricedItem.FromLot(CreateLot("Lot", 50m), 1, 0.1m) };

            Assert.Throws<ArgumentNullException>(() => Order.Create(Guid.NewGuid(), null!, payment, items));
        }

        [Fact]
        public void Create_WithNullPaymentInfo_ThrowsArgumentNullException()
        {
            var shipping = CreateShippingInfo();
            var items = new[] { PricedItem.FromLot(CreateLot("Lot", 50m), 1, 0.1m) };

            Assert.Throws<ArgumentNullException>(() => Order.Create(Guid.NewGuid(), shipping, null!, items));
        }

        [Fact]
        public void Create_WithNullItems_ThrowsArgumentException()
        {
            var shipping = CreateShippingInfo();
            var payment = CreatePaymentInfo();

            Assert.Throws<ArgumentException>(() => Order.Create(Guid.NewGuid(), shipping, payment, null!));
        }

        [Fact]
        public void Create_WithEmptyItems_ThrowsArgumentException()
        {
            var shipping = CreateShippingInfo();
            var payment = CreatePaymentInfo();

            Assert.Throws<ArgumentException>(() => Order.Create(Guid.NewGuid(), shipping, payment, Array.Empty<PricedItem>()));
        }

        [Fact]
        public void Create_WithMixedCurrencies_ThrowsInvalidOperationException()
        {
            var shipping = CreateShippingInfo();
            var payment = CreatePaymentInfo();
            var items = new[]
            {
                PricedItem.FromLot(CreateLot("Lot A", 100m, currency: Currency.USD), 1, 0.05m),
                PricedItem.FromLot(CreateLot("Lot B", 90m, currency: Currency.EUR), 1, 0.05m)
            };

            Assert.Throws<InvalidOperationException>(() => Order.Create(Guid.NewGuid(), shipping, payment, items));
        }

        [Fact]
        public void MarkPaid_FromCreated_SetsStatusToPaid()
        {
            var order = CreateOrder();

            order.MarkPaid();

            Assert.Equal(OrderStatus.Paid, order.Status);
        }

        [Fact]
        public void MarkPaid_WhenAlreadyPaid_ThrowsInvalidOperationException()
        {
            var order = CreateOrder();
            order.MarkPaid();

            Assert.Throws<InvalidOperationException>(() => order.MarkPaid());
        }

        private static Order CreateOrder()
        {
            var items = new[] { PricedItem.FromLot(CreateLot("Lot", 60m), 1, 0.05m) };
            return Order.Create(Guid.NewGuid(), CreateShippingInfo(), CreatePaymentInfo(), items);
        }

        private static ShippingInfo CreateShippingInfo() =>
            new("John Doe", "123 Main St", null, "Metropolis", "NY", "12345", "USA", "+1234567890");

        private static PaymentInfo CreatePaymentInfo() =>
            new("FakeGateway", 0.05m, "https://example.com/return");

        private static Lot CreateLot(string name, decimal priceAmount, Currency currency = Currency.USD, Category? category = null)
        {
            var price = new Money(priceAmount, currency);
            var compensation = new Money(Math.Max(priceAmount - 20m, 0m), currency);
            var discount = new Money(Math.Max(priceAmount - 10m, 0m), currency);

            return new Lot(
                Guid.NewGuid(),
                name,
                $"{name} description",
                price,
                compensation,
                stockCount: 10,
                discountedPrice: discount,
                LotType.Simple,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                category ?? CreateCategory());
        }

        private static Category CreateCategory()
        {
            return new Category(Guid.NewGuid(), "Test Category", "Test category description");
        }

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller", "Valid seller", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}