using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class TicketsServiceTests
    {
        [Fact]
        public async Task GetAll_ForwardsToRepository()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var expected = new List<Ticket>
            {
                Ticket.Create(fixture.Create<Guid>(), lotId)
            }.AsReadOnly();

            var repo = fixture.Freeze<Mock<ITicketsRepository>>();
            repo.Setup(r => r.GetAll(lotId)).ReturnsAsync(expected);

            var service = fixture.Create<TicketsService>();

            var result = await service.GetAll(lotId);

            Assert.Same(expected, result);
            repo.Verify(r => r.GetAll(lotId), Times.Once);
        }

        [Fact]
        public async Task Create_WhenLotIsNotDrawLot_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(CreateSimpleLot(id: lotId));

            var service = fixture.Create<TicketsService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.Create(lotId, 1));
        }

        [Fact]
        public async Task Create_WithoutUserIdClaim_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(new ClaimsPrincipal(new ClaimsIdentity()));

            var lotId = fixture.Create<Guid>();
            var drawLot = CreateDrawLot(id: lotId, ticketsSold: 0);

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(drawLot);

            var service = fixture.Create<TicketsService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.Create(lotId, 1));
        }

        [Fact]
        public async Task Create_WithValidDrawLot_CreatesTicketsAndUpdatesLot()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipalWithNameIdentifier(userId));

            var lotId = fixture.Create<Guid>();
            var drawLot = CreateDrawLot(id: lotId, ticketsSold: 0);

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(drawLot);

            IReadOnlyCollection<Ticket>? created = null;
            var ticketsRepo = fixture.Freeze<Mock<ITicketsRepository>>();
            ticketsRepo.Setup(r => r.Create(It.IsAny<IReadOnlyCollection<Ticket>>()))
                .Callback<IReadOnlyCollection<Ticket>>(t => created = t)
                .Returns(Task.CompletedTask);

            var service = fixture.Create<TicketsService>();

            var result = await service.Create(lotId, 2);

            Assert.Equal(2, result.Count);
            Assert.NotNull(created);
            Assert.Equal(2, created!.Count);
            Assert.All(created, t => Assert.Equal(userId, t.UserId));

            ticketsRepo.Verify(r => r.Create(It.IsAny<IReadOnlyCollection<Ticket>>()), Times.Once);
            lotsRepo.Verify(r => r.UpdateLot(lotId, It.Is<Lot>(l => l.GetType() == typeof(DrawLot) && ((DrawLot)l).TicketsSold == 2)), Times.Once);
        }

        [Fact]
        public async Task FindWinner_WhenLotIsNotDrawLot_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(CreateSimpleLot(id: lotId));

            var service = fixture.Create<TicketsService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.FindWinner(lotId));
        }

        [Fact]
        public async Task FindWinner_WithValidDrawLot_MarksWinnerAndUpdatesLot()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var drawLot = CreateDrawLot(id: lotId, ticketsSold: 10);

            var tickets = Enumerable.Range(0, 10)
                .Select(_ => Ticket.Create(Guid.NewGuid(), lotId).MarkAsPayed())
                .ToList()
                .AsReadOnly();

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(drawLot);

            var ticketsRepo = fixture.Freeze<Mock<ITicketsRepository>>();
            ticketsRepo.Setup(r => r.GetAll(lotId)).ReturnsAsync(tickets);

            IReadOnlyCollection<Ticket>? updatedTickets = null;
            ticketsRepo.Setup(r => r.Update(It.IsAny<IReadOnlyCollection<Ticket>>()))
                .Callback<IReadOnlyCollection<Ticket>>(x => updatedTickets = x)
                .Returns(Task.CompletedTask);

            var service = fixture.Create<TicketsService>();

            var winner = await service.FindWinner(lotId);

            Assert.NotNull(updatedTickets);
            Assert.Single(updatedTickets.Where(t => t.IsWinning));
            Assert.Equal(winner.Id, updatedTickets.Single(t => t.IsWinning).Id);

            ticketsRepo.Verify(r => r.GetAll(lotId), Times.Once);
            ticketsRepo.Verify(r => r.Update(It.IsAny<IReadOnlyCollection<Ticket>>()), Times.Once);
            lotsRepo.Verify(r => r.UpdateLot(lotId, It.Is<Lot>(l => l.GetType() == typeof(DrawLot))), Times.Once);
        }

        [Fact]
        public async Task FindWinner_WhenTicketsAreNotAllPayed_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var drawLot = CreateDrawLot(id: lotId, ticketsSold: 10);

            var tickets = Enumerable.Range(0, 10)
                .Select(_ => Ticket.Create(Guid.NewGuid(), lotId))
                .ToList()
                .AsReadOnly();

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(drawLot);

            var ticketsRepo = fixture.Freeze<Mock<ITicketsRepository>>();
            ticketsRepo.Setup(r => r.GetAll(lotId)).ReturnsAsync(tickets);

            var service = fixture.Create<TicketsService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.FindWinner(lotId));
        }

        [Fact]
        public async Task MarkAsPayedByOrderId_WithEmptyOrderId_ThrowsArgumentException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var service = fixture.Create<TicketsService>();

            await Assert.ThrowsAsync<ArgumentException>(() => service.MarkAsPayedByOrderId(Guid.Empty));
        }

        [Fact]
        public async Task MarkAsPayedByOrderId_WhenOrderNotFound_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var orderId = fixture.Create<Guid>();
            fixture.Freeze<Mock<IOrdersRepository>>()
                .Setup(r => r.GetById(orderId))
                .ReturnsAsync((Order?)null);

            var service = fixture.Create<TicketsService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.MarkAsPayedByOrderId(orderId));
        }

        [Fact]
        public async Task MarkAsPayedByOrderId_WithDrawOrder_MarksTicketsUsingDomainAndUpdatesRepository()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithNameIdentifier(fixture.Create<Guid>()));

            var customerId = Guid.NewGuid();
            var lotId = Guid.NewGuid();
            var order = CreateOrder(customerId, (lotId, 2));

            var drawLot = CreateDrawLot(id: lotId, ticketsSold: 3);

            var t1 = new Ticket(Guid.NewGuid(), customerId, lotId, createdAt: DateTime.UtcNow.AddMinutes(-3), isPayed: false);
            var t2 = new Ticket(Guid.NewGuid(), customerId, lotId, createdAt: DateTime.UtcNow.AddMinutes(-2), isPayed: false);
            var t3 = new Ticket(Guid.NewGuid(), Guid.NewGuid(), lotId, createdAt: DateTime.UtcNow.AddMinutes(-1), isPayed: false);

            var ordersRepo = fixture.Freeze<Mock<IOrdersRepository>>();
            ordersRepo.Setup(r => r.GetById(order.Id)).ReturnsAsync(order);

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(drawLot);

            var ticketsRepo = fixture.Freeze<Mock<ITicketsRepository>>();
            ticketsRepo.Setup(r => r.GetAll(lotId)).ReturnsAsync(new[] { t1, t2, t3 });

            IReadOnlyCollection<Ticket>? updated = null;
            ticketsRepo.Setup(r => r.Update(It.IsAny<IReadOnlyCollection<Ticket>>()))
                .Callback<IReadOnlyCollection<Ticket>>(x => updated = x)
                .Returns(Task.CompletedTask);

            var service = fixture.Create<TicketsService>();

            await service.MarkAsPayedByOrderId(order.Id);

            Assert.NotNull(updated);
            Assert.Equal(2, updated!.Count);
            Assert.All(updated, x =>
            {
                Assert.Equal(customerId, x.UserId);
                Assert.True(x.IsPayed);
            });

            ticketsRepo.Verify(r => r.GetAll(lotId), Times.Once);
            ticketsRepo.Verify(r => r.Update(It.IsAny<IReadOnlyCollection<Ticket>>()), Times.Once);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ClaimsPrincipal CreatePrincipalWithNameIdentifier(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            return new ClaimsPrincipal(identity);
        }

        private static Lot CreateSimpleLot(Guid? id = null)
        {
            return new Lot(
                id ?? Guid.NewGuid(),
                "Simple lot",
                "Simple description",
                new Money(100m, Currency.USD),
                new Money(60m, Currency.USD),
                stockCount: 5,
                discountedPrice: null,
                LotType.Simple,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                category: CreateCategory());
        }

        private static DrawLot CreateDrawLot(Guid? id = null, int ticketsSold = 0)
        {
            return new DrawLot(
                id ?? Guid.NewGuid(),
                "Draw lot",
                "Draw description",
                new Money(100m, Currency.USD),
                new Money(60m, Currency.USD),
                stockCount: 1,
                discountedPrice: null,
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: new Money(10m, Currency.USD),
                ticketsSold: ticketsSold,
                category: CreateCategory());
        }

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller", "Description", "seller@example.com", "+12345678901", null, Guid.NewGuid());

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category", "Category description");

        private static Order CreateOrder(Guid customerId, params (Guid LotId, int Quantity)[] items)
        {
            var shipping = new ShippingAddress(
                "Buyer",
                "Line1",
                null,
                "City",
                "State",
                "12345",
                "US",
                "+12345678901");

            var payment = new PaymentInfo("Stripe", 0.07m, "https://example.com/return");

            var pricedItems = items
                .Select(i => PricedItem.FromCustomPrice(i.LotId, $"Lot-{i.LotId}", new Money(10m, Currency.USD), i.Quantity, 0m))
                .ToList();

            return Order.Create(customerId, CreateSeller(), shipping, payment, pricedItems);
        }
    }
}