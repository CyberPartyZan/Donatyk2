namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class CartTests
    {
        [Fact]
        public void Constructor_WithNullItems_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Cart(null!));
        }

        [Fact]
        public void Constructor_WithItemsFromDifferentUsers_ThrowsArgumentException()
        {
            var items = new[]
            {
                CreateCartItem(Guid.NewGuid()),
                CreateCartItem(Guid.NewGuid())
            };

            Assert.Throws<ArgumentException>(() => new Cart(items));
        }

        [Fact]
        public void Constructor_WithEmptyItems_AllowsCartCreation()
        {
            var cart = new Cart(Array.Empty<CartItem>());

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void Constructor_WithValidItems_MaterializesReadOnlyCollection()
        {
            var userId = Guid.NewGuid();
            var items = new[]
            {
                CreateCartItem(userId),
                CreateCartItem(userId)
            };

            var cart = new Cart(items);

            Assert.Equal(items.Length, cart.Items.Count);
            Assert.All(cart.Items, item => Assert.Equal(userId, item.UserId));
        }

        private static CartItem CreateCartItem(Guid userId) =>
            new(CreateLot(), quantity: 1, userId);

        private static Lot CreateLot()
        {
            var category = CreateCategory();

            return new(
                Guid.NewGuid(),
                "Lot name",
                "Lot description",
                new Money(100m, Currency.USD),
                new Money(80m, Currency.USD),
                stockCount: 1,
                discountedPrice: new Money(90m, Currency.USD),
                LotType.Simple,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                category: category);
        }

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category name", "Category description");

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}
