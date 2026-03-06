namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class DrawLotTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsTicketPriceAndBaseProperties()
        {
            var id = Guid.NewGuid();
            var seller = CreateSeller();
            var price = CreateMoney(75m);
            var compensation = CreateMoney(40m);
            var discountedPrice = CreateMoney(70m);
            var ticketPrice = CreateMoney(5m);
            const string declineReason = "Need better docs";

            var lot = new DrawLot(
                id,
                "Charity raffle",
                "Win a signed poster",
                price,
                compensation,
                stockCount: 100,
                discountedPrice,
                LotType.Draw,
                LotStage.PendingApproval,
                seller,
                isActive: true,
                isCompensationPaid: false,
                ticketPrice,
                declineReason);

            Assert.Equal(id, lot.Id);
            Assert.Equal(ticketPrice, lot.TicketPrice);
            Assert.Equal(price - compensation, lot.Profit);
            Assert.Equal(declineReason, lot.DeclineReason);
        }

        [Fact]
        public void Constructor_WithNullTicketPrice_ThrowsArgumentOutOfRangeException()
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DrawLot(
                    Guid.NewGuid(),
                    "Raffle",
                    "Description",
                    CreateMoney(50m),
                    CreateMoney(25m),
                    stockCount: 10,
                    discountedPrice: CreateMoney(45m),
                    LotType.Draw,
                    LotStage.PendingApproval,
                    seller,
                    isActive: true,
                    isCompensationPaid: false,
                    ticketPrice: null!));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Constructor_WithNonPositiveTicketPrice_ThrowsArgumentOutOfRangeException(decimal ticketPriceAmount)
        {
            var seller = CreateSeller();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DrawLot(
                    Guid.NewGuid(),
                    "Raffle",
                    "Description",
                    CreateMoney(50m),
                    CreateMoney(25m),
                    stockCount: 10,
                    discountedPrice: CreateMoney(45m),
                    LotType.Draw,
                    LotStage.PendingApproval,
                    seller,
                    isActive: true,
                    isCompensationPaid: false,
                    new Money(ticketPriceAmount, Currency.USD)));
        }

        private static Money CreateMoney(decimal amount) => new(amount, Currency.USD);

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}
