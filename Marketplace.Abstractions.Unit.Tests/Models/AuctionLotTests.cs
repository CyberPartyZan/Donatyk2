namespace Marketplace.Abstractions.Unit.Tests
{
    public sealed class AuctionLotTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsAuctionSpecificState()
        {
            var id = Guid.NewGuid();
            var seller = CreateSeller();
            var price = CreateMoney(250m);
            var compensation = CreateMoney(150m);
            var endOfAuction = DateTime.UtcNow.AddHours(4);
            const int stepPercent = 10;
            const double discount = 5d;
            const string declineReason = "Needs better photos";

            var lot = new AuctionLot(
                id,
                "Rare collectible",
                "Genuine limited edition item",
                price,
                compensation,
                stockCount: 1,
                discount,
                LotType.Auction,
                LotStage.PendingApproval,
                seller,
                isActive: true,
                isCompensationPaid: false,
                endOfAuction,
                stepPercent,
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
            var pastEnd = DateTime.UtcNow.AddMinutes(-5);

            Assert.Throws<ArgumentException>(() =>
                new AuctionLot(
                    Guid.NewGuid(),
                    "Any lot",
                    "Description",
                    CreateMoney(120m),
                    CreateMoney(80m),
                    stockCount: 1,
                    discount: 0,
                    LotType.Auction,
                    LotStage.PendingApproval,
                    seller,
                    isActive: true,
                    isCompensationPaid: false,
                    pastEnd,
                    auctionStepPercent: 5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void Constructor_WithNonPositiveStepPercent_ThrowsArgumentOutOfRangeException(int stepPercent)
        {
            var seller = CreateSeller();
            var futureEnd = DateTime.UtcNow.AddHours(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new AuctionLot(
                    Guid.NewGuid(),
                    "Any lot",
                    "Description",
                    CreateMoney(120m),
                    CreateMoney(80m),
                    stockCount: 1,
                    discount: 0,
                    LotType.Auction,
                    LotStage.PendingApproval,
                    seller,
                    isActive: true,
                    isCompensationPaid: false,
                    futureEnd,
                    stepPercent));
        }

        private static Money CreateMoney(decimal amount) => new(amount, Currency.USD);

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}
