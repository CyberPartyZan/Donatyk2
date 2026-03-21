namespace Marketplace.Abstractions.Unit.Tests
{
    public sealed class AuctionLotTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsAuctionSpecificState()
        {
            var id = Guid.NewGuid();
            var seller = CreateSeller();
            var category = CreateCategory();
            var price = CreateMoney(250m);
            var compensation = CreateMoney(150m);
            var discountedPrice = CreateMoney(230m);
            var endOfAuction = DateTime.UtcNow.AddHours(4);
            const int stepPercent = 10;
            const string declineReason = "Needs better photos";

            var lot = new AuctionLot(
                id,
                "Rare collectible",
                "Genuine limited edition item",
                price,
                compensation,
                stockCount: 1,
                discountedPrice,
                LotType.Auction,
                LotStage.PendingApproval,
                seller,
                isActive: true,
                isCompensationPaid: false,
                endOfAuction,
                stepPercent,
                category,
                declineReason);

            Assert.Equal(id, lot.Id);
            Assert.Equal(price, lot.Price);
            Assert.Equal(compensation, lot.Compensation);
            Assert.Equal(LotType.Auction, lot.Type);
            Assert.Equal(LotStage.PendingApproval, lot.Stage);
            Assert.Equal(endOfAuction, lot.EndOfAuction);
            Assert.Equal(stepPercent, lot.AuctionStepPercent);
            Assert.Equal(declineReason, lot.DeclineReason);
            Assert.Equal(seller, lot.Seller);
        }

        [Fact]
        public void Constructor_WithPastEndOfAuction_ThrowsArgumentException()
        {
            var seller = CreateSeller();
            var category = CreateCategory();
            var pastEnd = DateTime.UtcNow.AddMinutes(-5);

            Assert.Throws<ArgumentException>(() =>
                new AuctionLot(
                    Guid.NewGuid(),
                    "Any lot",
                    "Description",
                    CreateMoney(120m),
                    CreateMoney(80m),
                    stockCount: 1,
                    discountedPrice: CreateMoney(110m),
                    LotType.Auction,
                    LotStage.PendingApproval,
                    seller,
                    isActive: true,
                    isCompensationPaid: false,
                    pastEnd,
                    auctionStepPercent: 5,
                    category));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void Constructor_WithNonPositiveStepPercent_ThrowsArgumentOutOfRangeException(int stepPercent)
        {
            var seller = CreateSeller();
            var category = CreateCategory();
            var futureEnd = DateTime.UtcNow.AddHours(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new AuctionLot(
                    Guid.NewGuid(),
                    "Any lot",
                    "Description",
                    CreateMoney(120m),
                    CreateMoney(80m),
                    stockCount: 1,
                    discountedPrice: CreateMoney(110m),
                    LotType.Auction,
                    LotStage.PendingApproval,
                    seller,
                    isActive: true,
                    isCompensationPaid: false,
                    futureEnd,
                    stepPercent,
                    category));
        }

        [Fact]
        public void Delete_WhenBidHistoryExists_ThrowsInvalidOperationException()
        {
            var bidHistory = new List<Bid>
            {
                new Bid(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreateMoney(350m), DateTime.UtcNow)
            };

            var lot = new AuctionLot(
                Guid.NewGuid(),
                "Auction lot",
                "Auction description",
                CreateMoney(300m),
                CreateMoney(150m),
                stockCount: 1,
                discountedPrice: null,
                LotType.Auction,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                DateTime.UtcNow.AddHours(2),
                auctionStepPercent: 5,
                category: CreateCategory(),
                bidHistory: bidHistory);

            Assert.Throws<InvalidOperationException>(() => lot.Delete());
        }

        [Fact]
        public void Delete_WhenNoBidHistory_SetsIsDeletedTrue()
        {
            var lot = new AuctionLot(
                Guid.NewGuid(),
                "Auction lot",
                "Auction description",
                CreateMoney(300m),
                CreateMoney(150m),
                stockCount: 1,
                discountedPrice: null,
                LotType.Auction,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                DateTime.UtcNow.AddHours(2),
                auctionStepPercent: 5,
                category: CreateCategory());

            lot.Delete();

            Assert.True(lot.IsDeleted);
        }

        [Fact]
        public void Sell_WhenAuctionEnded_ReducesStock()
        {
            var lot = new AuctionLot(
                Guid.NewGuid(),
                "Auction lot",
                "Auction description",
                CreateMoney(300m),
                CreateMoney(150m),
                stockCount: 2,
                discountedPrice: null,
                LotType.Auction,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                DateTime.UtcNow.AddHours(1),
                auctionStepPercent: 5,
                category: CreateCategory());

            lot.EndOfAuction = DateTime.UtcNow.AddMinutes(-1);

            lot.Sell(1);

            Assert.Equal(1, lot.StockCount);
        }

        private static Money CreateMoney(decimal amount) => new(amount, Currency.USD);

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category name", "Category description");
    }
}
