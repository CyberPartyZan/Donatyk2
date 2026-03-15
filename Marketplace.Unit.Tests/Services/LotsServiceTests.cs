using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class LotsServiceTests
    {
        [Fact]
        public async Task GetAll_WithLots_MapsSpecializedProperties()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var baseLot = CreateLot();
            var auctionLot = CreateAuctionLot();
            var drawLot = CreateDrawLot();
            var query = new LotSearchQuery { SearchText = "poster" };

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetAll(query))
               .ReturnsAsync(new List<Lot> { baseLot, auctionLot, drawLot });

            var service = fixture.Create<LotsService>();

            var result = (await service.GetAll(query)).ToList();

            Assert.Equal(3, result.Count);

            var baseDto = result[0];
            Assert.Equal(baseLot.Id, baseDto.Id);
            Assert.Equal(baseLot.Seller.Name, baseDto.Seller.Name);
            Assert.Null(baseDto.EndOfAuction);
            Assert.Null(baseDto.AuctionStepPercent);
            Assert.Null(baseDto.TicketPrice);

            var auctionDto = result[1];
            Assert.Equal(auctionLot.EndOfAuction, auctionDto.EndOfAuction);
            Assert.Equal(auctionLot.AuctionStepPercent, auctionDto.AuctionStepPercent);

            var drawDto = result[2];
            Assert.Equal(drawLot.TicketPrice, drawDto.TicketPrice);

            repo.Verify(r => r.GetAll(query), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenQueryIsNull_PassesDefaultQuery()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetAll(It.IsAny<LotSearchQuery>()))
                .ReturnsAsync(Array.Empty<Lot>());

            var service = fixture.Create<LotsService>();

            await service.GetAll(null!);

            repo.Verify(r => r.GetAll(It.Is<LotSearchQuery>(q => q != null)), Times.Once);
        }

        [Fact]
        public async Task GetLotById_WhenMissing_ReturnsNull()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(It.IsAny<Guid>()))
                .ReturnsAsync((Lot?)null);

            var service = fixture.Create<LotsService>();

            var result = await service.GetLotById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetLotById_WhenFound_ReturnsDto()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var lot = CreateAuctionLot();
            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(lot.Id)).ReturnsAsync(lot);

            var service = fixture.Create<LotsService>();

            var result = await service.GetLotById(lot.Id);

            Assert.NotNull(result);
            Assert.Equal(lot.Id, result!.Id);
            Assert.Equal(lot.Seller.Name, result.Seller.Name);
            Assert.Equal(lot.EndOfAuction, result.EndOfAuction);
        }

        [Fact]
        public async Task CreateLot_WithNullDto_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var service = fixture.Create<LotsService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateLot(null!));
        }

        [Fact]
        public async Task CreateLot_WithoutUserIdClaim_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(new ClaimsPrincipal(new ClaimsIdentity()));

            var service = fixture.Create<LotsService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateLot(CreateLotDto()));
        }

        [Fact]
        public async Task CreateLot_WithValidDto_SetsPendingStageAndCurrentUser()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var dto = CreateLotDto();
            dto.Stage = LotStage.Approved; // should be overridden

            Lot? capturedLot = null;
            var expectedId = fixture.Create<Guid>();

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.CreateLot(It.IsAny<Lot>()))
                .Callback<Lot>(lot => capturedLot = lot)
                .ReturnsAsync(expectedId);

            var service = fixture.Create<LotsService>();

            var result = await service.CreateLot(dto);

            Assert.Equal(expectedId, result);
            Assert.NotNull(capturedLot);
            Assert.Equal(LotStage.PendingApproval, capturedLot!.Stage);
            Assert.Equal(userId, capturedLot.Seller.UserId);
            Assert.Equal(dto.Name, capturedLot.Name);
        }

        [Fact]
        public async Task UpdateLot_WithNullDto_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var service = fixture.Create<LotsService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateLot(Guid.NewGuid(), null!));
        }

        [Fact]
        public async Task UpdateLot_WhenLotMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(It.IsAny<Guid>()))
                .ReturnsAsync((Lot?)null);

            var service = fixture.Create<LotsService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateLot(Guid.NewGuid(), CreateLotDto()));
        }

        [Fact]
        public async Task UpdateLot_WithExistingLot_PreservesSellerUserId()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(Guid.NewGuid())); // current user should not override seller id

            var existing = CreateLot(stage: LotStage.PendingApproval, sellerUserId: userId);
            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(existing.Id)).ReturnsAsync(existing);

            Lot? captured = null;
            repo.Setup(r => r.UpdateLot(existing.Id, It.IsAny<Lot>()))
                .Callback<Guid, Lot>((_, lot) => captured = lot)
                .Returns(Task.CompletedTask);

            var dto = CreateLotDto();
            dto.Stage = LotStage.Approved;

            var service = fixture.Create<LotsService>();

            await service.UpdateLot(existing.Id, dto);

            Assert.NotNull(captured);
            Assert.Equal(userId, captured!.Seller.UserId);
            Assert.Equal(dto.Stage, captured.Stage);
            Assert.Equal(dto.Name, captured.Name);
        }

        [Fact]
        public async Task DeleteLot_InvokesRepository()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            var service = fixture.Create<LotsService>();
            var id = fixture.Create<Guid>();

            await service.DeleteLot(id);

            repo.Verify(r => r.DeleteLot(id), Times.Once);
        }

        [Fact]
        public async Task ApproveLot_WhenMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(It.IsAny<Guid>())).ReturnsAsync((Lot?)null);

            var service = fixture.Create<LotsService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ApproveLot(Guid.NewGuid()));
        }

        [Fact]
        public async Task ApproveLot_WhenAlreadyApproved_DoesNothing()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var lot = CreateLot(stage: LotStage.Approved);
            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(lot.Id)).ReturnsAsync(lot);

            var service = fixture.Create<LotsService>();

            await service.ApproveLot(lot.Id);

            repo.Verify(r => r.ApproveLot(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ApproveLot_WhenPending_CallsRepository()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var lot = CreateLot(stage: LotStage.PendingApproval);
            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(lot.Id)).ReturnsAsync(lot);

            var service = fixture.Create<LotsService>();

            await service.ApproveLot(lot.Id);

            repo.Verify(r => r.ApproveLot(lot.Id), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeclineLot_WithInvalidReason_ThrowsArgumentException(string? reason)
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var service = fixture.Create<LotsService>();

            await Assert.ThrowsAsync<ArgumentException>(() => service.DeclineLot(Guid.NewGuid(), reason!));
        }

        [Fact]
        public async Task DeclineLot_WhenMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(It.IsAny<Guid>())).ReturnsAsync((Lot?)null);

            var service = fixture.Create<LotsService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeclineLot(Guid.NewGuid(), "Reason"));
        }

        [Fact]
        public async Task DeclineLot_WhenAlreadyDeniedWithSameReason_DoesNothing()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            const string declineReason = "duplicates";
            var lot = CreateLot(stage: LotStage.Denied, declineReason: declineReason);
            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(lot.Id)).ReturnsAsync(lot);

            var service = fixture.Create<LotsService>();

            await service.DeclineLot(lot.Id, "  duplicates  ");

            repo.Verify(r => r.DeclineLot(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeclineLot_WithNewReason_TrimmedReasonPassedToRepository()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var lot = CreateLot(stage: LotStage.PendingApproval);
            var repo = fixture.Freeze<Mock<ILotsRepository>>();
            repo.Setup(r => r.GetLotById(lot.Id)).ReturnsAsync(lot);

            var service = fixture.Create<LotsService>();

            await service.DeclineLot(lot.Id, "  missing docs ");

            repo.Verify(r => r.DeclineLot(lot.Id, "missing docs"), Times.Once);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ClaimsPrincipal CreatePrincipal(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            return new ClaimsPrincipal(identity);
        }

        private static Lot CreateLot(
            LotStage stage = LotStage.Created,
            Guid? sellerUserId = null,
            string? declineReason = null,
            LotType type = LotType.Simple)
        {
            return new Lot(
                Guid.NewGuid(),
                "Lot " + Guid.NewGuid().ToString("N"),
                "Description " + Guid.NewGuid().ToString("N"),
                CreateMoney(200m),
                CreateMoney(100m),
                stockCount: 5,
                discountedPrice: CreateMoney(200m),
                type: type,
                stage: stage,
                seller: CreateSeller(sellerUserId),
                isActive: true,
                isCompensationPaid: false,
                category: CreateCategory(),
                declineReason: declineReason);
        }

        private static AuctionLot CreateAuctionLot()
        {
            return new AuctionLot(
                Guid.NewGuid(),
                "Auction lot",
                "Auction description",
                CreateMoney(300m),
                CreateMoney(150m),
                stockCount: 3,
                discountedPrice: CreateMoney(300m),
                LotType.Auction,
                LotStage.PendingApproval,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                DateTime.UtcNow.AddHours(2),
                auctionStepPercent: 5,
                category: CreateCategory());
        }

        private static DrawLot CreateDrawLot()
        {
            return new DrawLot(
                Guid.NewGuid(),
                "Draw lot",
                "Draw description",
                CreateMoney(120m),
                CreateMoney(60m),
                stockCount: 50,
                discountedPrice: CreateMoney(120m),
                LotType.Draw,
                LotStage.Created,
                CreateSeller(),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(5m),
                category: CreateCategory());
        }

        private static LotDto CreateLotDto() =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Lot dto " + Guid.NewGuid().ToString("N"),
                Description = "Dto description " + Guid.NewGuid().ToString("N"),
                Price = CreateMoney(250m),
                Compensation = CreateMoney(150m),
                StockCount = 10,
                DiscountedPrice = CreateMoney(230m),
                Discount = 0,
                Type = LotType.Simple,
                Stage = LotStage.Created,
                Seller = new SellerDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Seller dto",
                    Description = "Seller dto description",
                    Email = "seller@example.com",
                    PhoneNumber = "+12345678901",
                    AvatarImageUrl = string.Empty
                },
                Category = new CategoryDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Category dto",
                    Description = "Category dto description"
                },
                IsActive = true,
                IsCompensationPaid = false,
                CreatedAt = DateTime.UtcNow
            };

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category", "Category description");

        private static Money CreateMoney(decimal amount) =>
            new(amount, Currency.USD);

        private static Seller CreateSeller(Guid? userId = null) =>
            new(
                Guid.NewGuid(),
                "Seller " + Guid.NewGuid().ToString("N"),
                "Seller description",
                "seller@example.com",
                "+1234567890",
                string.Empty,
                userId ?? Guid.NewGuid());
    }
}