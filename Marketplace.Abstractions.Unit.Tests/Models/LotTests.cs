namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class LotTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsPropertiesAndProfit()
        {
            var id = Guid.NewGuid();
            var seller = CreateSeller();
            var category = CreateCategory();
            var price = new Money(150m, Currency.USD);
            var compensation = new Money(110m, Currency.USD);
            var stockCount = 5;
            var discountedPrice = new Money(120m, Currency.USD);
            const LotType type = LotType.Auction;
            const LotStage stage = LotStage.PendingApproval;
            const bool isActive = true;
            const bool isCompensationPaid = false;

            var lot = new Lot(id, "Limited edition cap", "Signed by the author", price, compensation, stockCount, discountedPrice, type, stage, seller, isActive, isCompensationPaid, category);

            Assert.Equal(id, lot.Id);
            Assert.Equal("Limited edition cap", lot.Name);
            Assert.Equal("Signed by the author", lot.Description);
            Assert.Equal(price, lot.Price);
            Assert.Equal(compensation, lot.Compensation);
            Assert.Equal(stockCount, lot.StockCount);
            Assert.Equal(discountedPrice, lot.DiscountedPrice);
            Assert.Equal(20d, lot.Discount);
            Assert.Equal(type, lot.Type);
            Assert.Equal(stage, lot.Stage);
            Assert.Equal(seller, lot.Seller);
            Assert.True(lot.IsActive);
            Assert.False(lot.IsCompensationPaid);
            Assert.Equal(price - compensation, lot.Profit);
        }

        [Fact]
        public void Constructor_WithNullDiscountedPrice_TreatsAsNoDiscount()
        {
            var price = CreateMoney(100m);
            var compensation = CreateMoney(70m);
            var lot = new Lot(
                Guid.NewGuid(),
                "No discount",
                "No discount description",
                price,
                compensation,
                stockCount: 3,
                discountedPrice: null,
                LotType.Simple,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                category: CreateCategory());

            Assert.Null(lot.DiscountedPrice);
            Assert.Equal(0d, lot.Discount);
        }

        [Fact]
        public void Constructor_WithNullCategory_ThrowsArgumentNullException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentNullException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), 1, CreateMoney(10m), LotType.Simple, LotStage.Created, seller, true, false, category: null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentException>(() =>
                new Lot(Guid.NewGuid(), name!, "Valid description", CreateMoney(100m), CreateMoney(80m), 1, CreateMoney(100m), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidDescription_ThrowsArgumentException(string? description)
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentException>(() =>
                new Lot(Guid.NewGuid(), "Valid name", description!, CreateMoney(100m), CreateMoney(80m), 1, CreateMoney(100m), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        [Fact]
        public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", new Money(-1m, Currency.USD), CreateMoney(0m), 1, CreateMoney(0m), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        [Fact]
        public void Constructor_WithNegativeCompensation_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), new Money(-1m, Currency.USD), 1, CreateMoney(10m), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        [Fact]
        public void Constructor_WithNegativeStockCount_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), -1, CreateMoney(10m), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        [Fact]
        public void Constructor_WithNegativeDiscountedPrice_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), 1, new Money(-1m, Currency.USD), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        [Fact]
        public void Constructor_WithDiscountedPriceAbovePrice_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), 1, CreateMoney(15m), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        [Fact]
        public void Constructor_WithNullSeller_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(10m), CreateMoney(5m), 1, CreateMoney(10m), LotType.Simple, LotStage.Created, seller: null!, true, false, category: CreateCategory()));
        }

        [Fact]
        public void Constructor_WithPriceLowerThanCompensation_ThrowsArgumentException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentException>(() =>
                new Lot(Guid.NewGuid(), "Valid", "Valid", CreateMoney(50m), CreateMoney(60m), 1, CreateMoney(50m), LotType.Simple, LotStage.Created, seller, true, false, category: CreateCategory()));
        }

        private static Money CreateMoney(decimal amount) => new(amount, Currency.USD);

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category name", "Category description");
    }
}
