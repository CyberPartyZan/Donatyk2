namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class DrawLotTests
    {
        [Fact]
        public void Constructor_WithValidArguments_SetsTicketPriceAndBaseProperties()
        {
            var id = Guid.NewGuid();
            var seller = CreateSeller();
            var category = CreateCategory();
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
                ticketsSold: 0,
                category,
                declineReason);

            Assert.Equal(id, lot.Id);
            Assert.Equal(ticketPrice, lot.TicketPrice);
            Assert.Equal(0, lot.TicketsSold);
            Assert.Equal(15, lot.TotalTickets);
            Assert.Equal(15, lot.TicketsLeft);
            Assert.Equal(price - compensation, lot.Profit);
            Assert.Equal(declineReason, lot.DeclineReason);
        }

        [Fact]
        public void Constructor_WithTicketsSold_ComputesTicketsLeft()
        {
            var lot = new DrawLot(
                Guid.NewGuid(),
                "Raffle",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: CreateMoney(45m),
                LotType.Draw,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 4,
                category: CreateCategory());

            Assert.Equal(10, lot.TotalTickets);
            Assert.Equal(4, lot.TicketsSold);
            Assert.Equal(6, lot.TicketsLeft);
        }

        [Fact]
        public void Constructor_WithTicketsSoldAboveTotal_ThrowsArgumentOutOfRangeException()
        {
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
                    CreateSeller(),
                    isActive: true,
                    isCompensationPaid: false,
                    ticketPrice: CreateMoney(5m),
                    ticketsSold: 11,
                    category: CreateCategory()));
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
                    ticketPrice: null!,
                    category: CreateCategory()));
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
                    new Money(ticketPriceAmount, Currency.USD),
                    category: CreateCategory()));
        }

        [Fact]
        public void ReadyToDraw_WhenTicketsNotLoaded_ReturnsFalse()
        {
            var lot = new DrawLot(
                Guid.NewGuid(),
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 10,
                category: CreateCategory());

            Assert.False(lot.ReadyToDraw);
        }

        [Fact]
        public void ReadyToDraw_WhenAllTicketsSoldAndPayed_ReturnsTrue()
        {
            var lotId = Guid.NewGuid();
            var tickets = Enumerable.Range(0, 10)
                .Select(_ => Ticket.Create(Guid.NewGuid(), lotId).MarkAsPayed())
                .ToList();

            var lot = new DrawLot(
                lotId,
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 10,
                category: CreateCategory(),
                tickets: tickets);

            Assert.True(lot.ReadyToDraw);
        }

        [Fact]
        public void FindWinner_WhenNotAllTicketsPayed_ThrowsInvalidOperationException()
        {
            var lotId = Guid.NewGuid();

            var payedTickets = Enumerable.Range(0, 9)
                .Select(_ => Ticket.Create(Guid.NewGuid(), lotId).MarkAsPayed());

            var mixedTickets = payedTickets
                .Append(Ticket.Create(Guid.NewGuid(), lotId))
                .ToList();

            var lot = new DrawLot(
                lotId,
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 10,
                category: CreateCategory());

            lot.LoadTickets(mixedTickets);

            Assert.Throws<InvalidOperationException>(() => lot.FindWinner());
        }

        [Fact]
        public void FindWinner_WhenAllTicketsPayed_ReturnsWinnerAndSetsIsDrawn()
        {
            var lotId = Guid.NewGuid();
            var tickets = Enumerable.Range(0, 10)
                .Select(_ => Ticket.Create(Guid.NewGuid(), lotId).MarkAsPayed())
                .ToList();

            var lot = new DrawLot(
                lotId,
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 10,
                category: CreateCategory());

            lot.LoadTickets(tickets);

            var winner = lot.FindWinner();

            Assert.True(lot.IsDrawn);
            Assert.True(winner.IsWinning);
            Assert.True(winner.IsPayed);
        }

        [Fact]
        public void Delete_WhenTicketsSold_ThrowsInvalidOperationException()
        {
            var lot = new DrawLot(
                Guid.NewGuid(),
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 1,
                category: CreateCategory());

            Assert.Throws<InvalidOperationException>(() => lot.Delete());
        }

        [Fact]
        public void Delete_WhenNoTicketsSold_SetsIsDeletedTrue()
        {
            var lot = new DrawLot(
                Guid.NewGuid(),
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 0,
                category: CreateCategory());

            lot.Delete();

            Assert.True(lot.IsDeleted);
        }

        [Fact]
        public void Sell_WhenNotAllTicketsSold_ThrowsInvalidOperationException()
        {
            var lot = new DrawLot(
                Guid.NewGuid(),
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 10,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 9,
                category: CreateCategory());

            Assert.Throws<InvalidOperationException>(() => lot.Sell(1));
        }

        [Fact]
        public void Sell_WhenAllTicketsSoldAndWinnerExists_ReducesStock()
        {
            var lotId = Guid.NewGuid();
            var tickets = Enumerable.Range(0, 10)
                .Select(i => new Ticket(Guid.NewGuid(), Guid.NewGuid(), lotId, isWinning: i == 0))
                .ToList();

            var lot = new DrawLot(
                lotId,
                "Draw",
                "Description",
                CreateMoney(50m),
                CreateMoney(25m),
                stockCount: 3,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                ticketsSold: 10,
                category: CreateCategory(),
                tickets: tickets,
                isDrawn: true);

            lot.Sell(1);

            Assert.Equal(2, lot.StockCount);
        }

        private static Money CreateMoney(decimal amount) => new(amount, Currency.USD);

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller name", "Seller description", "seller@example.com", "+12345678901", null, Guid.NewGuid());

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category name", "Category description");
    }
}
