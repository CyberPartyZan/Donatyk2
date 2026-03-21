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
            lotsRepo.Verify(r => r.UpdateLot(lotId, It.Is<Lot>(l => l is DrawLot)), Times.Once);
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
                .Select(_ => Ticket.Create(Guid.NewGuid(), lotId))
                .ToList()
                .AsReadOnly();

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(drawLot);

            var ticketsRepo = fixture.Freeze<Mock<ITicketsRepository>>();
            ticketsRepo.Setup(r => r.GetAll(lotId)).ReturnsAsync(tickets);

            var service = fixture.Create<TicketsService>();

            var winner = await service.FindWinner(lotId);

            ticketsRepo.Verify(r => r.MarkAsWinning(lotId, winner.Id), Times.Once);
            lotsRepo.Verify(r => r.UpdateLot(lotId, It.Is<Lot>(l => l is DrawLot)), Times.Once);
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
    }
}