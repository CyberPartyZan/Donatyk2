using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class BidsServiceTests
    {
        [Fact]
        public async Task LoadBidHistory_ForwardsToRepository()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithSub(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var expected = new List<Bid>
            {
                new Bid(Guid.NewGuid(), lotId, Guid.NewGuid(), new Money(120m, Currency.USD), DateTime.UtcNow)
            }.AsReadOnly();

            var bidsRepo = fixture.Freeze<Mock<IBidsRepository>>();
            bidsRepo.Setup(r => r.LoadBidHistory(lotId)).ReturnsAsync(expected);

            var service = fixture.Create<BidsService>();

            var result = await service.LoadBidHistory(lotId);

            Assert.Same(expected, result);
            bidsRepo.Verify(r => r.LoadBidHistory(lotId), Times.Once);
        }

        [Fact]
        public async Task PlaceBid_WhenLotMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithSub(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync((Lot?)null);

            var service = fixture.Create<BidsService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.PlaceBid(lotId, new Money(120m, Currency.USD)));
        }

        [Fact]
        public async Task PlaceBid_WhenLotIsNotAuction_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithSub(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(CreateSimpleLot(id: lotId));

            var service = fixture.Create<BidsService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceBid(lotId, new Money(120m, Currency.USD)));
        }

        [Fact]
        public async Task PlaceBid_WithoutSubClaim_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(new ClaimsPrincipal(new ClaimsIdentity()));

            var lotId = fixture.Create<Guid>();
            var auction = CreateAuctionLot(id: lotId);

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(auction);

            var bidsRepo = fixture.Freeze<Mock<IBidsRepository>>();
            bidsRepo.Setup(r => r.LoadBidHistory(lotId)).ReturnsAsync(Array.Empty<Bid>());

            var service = fixture.Create<BidsService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceBid(lotId, new Money(120m, Currency.USD)));
        }

        [Fact]
        public async Task PlaceBid_WithValidAuction_PlacesBidAndUpdatesLot()
        {
            var fixture = CreateFixture();
            var bidderId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipalWithSub(bidderId));

            var lotId = fixture.Create<Guid>();
            var auction = CreateAuctionLot(id: lotId);

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(auction);

            var bidsRepo = fixture.Freeze<Mock<IBidsRepository>>();
            bidsRepo.Setup(r => r.LoadBidHistory(lotId)).ReturnsAsync(Array.Empty<Bid>());

            var ordersRepo = fixture.Freeze<Mock<IOrdersRepository>>();
            ordersRepo.Setup(r => r.GetPaidOrderByLotId(lotId, default)).ReturnsAsync((Order?)null);

            Bid? capturedBid = null;
            bidsRepo.Setup(r => r.PlaceBid(It.IsAny<Bid>()))
                .Callback<Bid>(b => capturedBid = b)
                .Returns(Task.CompletedTask);

            var service = fixture.Create<BidsService>();

            var placed = await service.PlaceBid(lotId, new Money(120m, Currency.USD));

            Assert.NotNull(capturedBid);
            Assert.Equal(bidderId, capturedBid!.BidderId);
            Assert.Equal(lotId, capturedBid.AuctionId);
            Assert.Equal(120m, capturedBid.Amount.Amount);
            Assert.Equal(capturedBid.Id, placed.Id);

            bidsRepo.Verify(r => r.PlaceBid(It.IsAny<Bid>()), Times.Once);
            lotsRepo.Verify(r => r.UpdateLot(lotId, It.Is<Lot>(l => l is AuctionLot)), Times.Once);
        }

        [Fact]
        public async Task PlaceBid_WhenPreviousHoldOrderExists_ReleasesHold()
        {
            var fixture = CreateFixture();
            var bidderId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipalWithSub(bidderId));

            var lotId = fixture.Create<Guid>();
            var auction = CreateAuctionLot(id: lotId);

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(auction);

            var bidsRepo = fixture.Freeze<Mock<IBidsRepository>>();
            bidsRepo.Setup(r => r.LoadBidHistory(lotId)).ReturnsAsync(Array.Empty<Bid>());

            var previousOrder = CreatePaidAuctionOrder(lotId);
            var ordersRepo = fixture.Freeze<Mock<IOrdersRepository>>();
            ordersRepo.Setup(r => r.GetPaidOrderByLotId(lotId, default)).ReturnsAsync(previousOrder);

            var paymentGateway = fixture.Freeze<Mock<IPaymentGateway>>();
            paymentGateway.Setup(g => g.ReleaseHoldAsync(previousOrder, default)).Returns(Task.CompletedTask);

            var service = fixture.Create<BidsService>();

            await service.PlaceBid(lotId, new Money(120m, Currency.USD));

            paymentGateway.Verify(g => g.ReleaseHoldAsync(previousOrder, default), Times.Once);
        }

        [Fact]
        public async Task PlaceBid_WhenNoPreviousHoldOrder_DoesNotCallReleaseHold()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipalWithSub(fixture.Create<Guid>()));

            var lotId = fixture.Create<Guid>();
            var auction = CreateAuctionLot(id: lotId);

            var lotsRepo = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepo.Setup(r => r.GetLotById(lotId)).ReturnsAsync(auction);

            var bidsRepo = fixture.Freeze<Mock<IBidsRepository>>();
            bidsRepo.Setup(r => r.LoadBidHistory(lotId)).ReturnsAsync(Array.Empty<Bid>());

            var ordersRepo = fixture.Freeze<Mock<IOrdersRepository>>();
            ordersRepo.Setup(r => r.GetPaidOrderByLotId(lotId, default)).ReturnsAsync((Order?)null);

            var paymentGateway = fixture.Freeze<Mock<IPaymentGateway>>();

            var service = fixture.Create<BidsService>();

            await service.PlaceBid(lotId, new Money(120m, Currency.USD));

            paymentGateway.Verify(g => g.ReleaseHoldAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ClaimsPrincipal CreatePrincipalWithSub(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));
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

        private static AuctionLot CreateAuctionLot(Guid? id = null)
        {
            return new AuctionLot(
                id ?? Guid.NewGuid(),
                "Auction lot",
                "Auction description",
                new Money(100m, Currency.USD),
                new Money(60m, Currency.USD),
                stockCount: 1,
                discountedPrice: null,
                LotType.Auction,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                endOfAuction: DateTime.UtcNow.AddHours(2),
                auctionStepPercent: 5,
                category: CreateCategory());
        }

        private static Order CreatePaidAuctionOrder(Guid lotId)
        {
            var item = PricedItem.FromCustomPrice(lotId, "Auction lot bid hold", new Money(100m, Currency.USD), 1, 0m);
            var order = Order.Create(
                Guid.NewGuid(),
                new ShippingInfo("John", "Doe", "john@example.com", "+12345678901", "123 St", "City", "12345", "US"),
                new PaymentInfo("TestProvider", 0m, returnUrl: null),
                new[] { item });
            order.MarkPaid();
            return order;
        }

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller", "Description", "seller@example.com", "+12345678901", null, Guid.NewGuid());

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category", "Category description");
    }
}