using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class CartServiceTests
    {
        [Fact]
        public async Task Get_WithAuthenticatedUser_ReturnsCartFromRepository()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            var expectedCart = new Cart(Array.Empty<CartItem>());
            fixture.Inject(CreatePrincipal(userId));

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            cartRepository.Setup(r => r.GetCartByUserId(userId)).ReturnsAsync(expectedCart);

            var service = fixture.Create<CartService>();

            var result = await service.Get();

            Assert.Same(expectedCart, result);
            cartRepository.Verify(r => r.GetCartByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task Get_WithoutUserIdClaim_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(new ClaimsPrincipal(new ClaimsIdentity()));

            var service = fixture.Create<CartService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.Get());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AddItem_WithNonPositiveQuantity_ThrowsArgumentOutOfRangeException(int quantity)
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            var lotsRepository = fixture.Freeze<Mock<ILotsRepository>>();

            var service = fixture.Create<CartService>();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.AddItem(Guid.NewGuid(), quantity));

            lotsRepository.Verify(r => r.GetLotById(It.IsAny<Guid>()), Times.Never);
            cartRepository.Verify(r => r.AddItem(It.IsAny<CartItem>()), Times.Never);
        }

        [Fact]
        public async Task AddItem_WhenLotDoesNotExist_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            var lotsRepository = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepository.Setup(r => r.GetLotById(It.IsAny<Guid>())).ReturnsAsync((Lot?)null);

            var service = fixture.Create<CartService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AddItem(Guid.NewGuid(), 1));

            cartRepository.Verify(r => r.AddItem(It.IsAny<CartItem>()), Times.Never);
        }

        [Fact]
        public async Task AddItem_WithValidInput_PersistsItemAndReturnsIdentifier()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            var lot = CreateLot();
            var quantity = 2;
            var expectedId = fixture.Create<Guid>();

            fixture.Inject(CreatePrincipal(userId));

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            cartRepository
                .Setup(r => r.AddItem(It.Is<CartItem>(item =>
                    item.UserId == userId &&
                    item.Quantity == quantity &&
                    item.Lot == lot)))
                .ReturnsAsync(expectedId);

            var lotsRepository = fixture.Freeze<Mock<ILotsRepository>>();
            lotsRepository.Setup(r => r.GetLotById(lot.Id)).ReturnsAsync(lot);

            var service = fixture.Create<CartService>();

            var result = await service.AddItem(lot.Id, quantity);

            Assert.Equal(expectedId, result);
            lotsRepository.Verify(r => r.GetLotById(lot.Id), Times.Once);
            cartRepository.Verify(r => r.AddItem(It.IsAny<CartItem>()), Times.Once);
        }

        [Fact]
        public async Task ChangeQuantity_ForwardsRequestToRepository()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            var service = fixture.Create<CartService>();

            var lotId = fixture.Create<Guid>();
            var quantity = Math.Max(1, Math.Abs(fixture.Create<int>()));

            await service.ChangeQuantity(lotId, quantity);

            cartRepository.Verify(r => r.ChangeQuantity(lotId, quantity, userId), Times.Once);
        }

        [Fact]
        public async Task RemoveItem_ForwardsRequestToRepository()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            var service = fixture.Create<CartService>();

            var lotId = fixture.Create<Guid>();

            await service.RemoveItem(lotId);

            cartRepository.Verify(r => r.RemoveItem(lotId, userId), Times.Once);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ClaimsPrincipal CreatePrincipal(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));
            return new ClaimsPrincipal(identity);
        }

        private static Lot CreateLot()
        {
            var seller = new Seller(Guid.NewGuid(), "Seller", "Description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
            var category = CreateCategory();

            return new Lot(
                Guid.NewGuid(),
                "Vintage poster",
                "Limited edition",
                new Money(150m, Currency.USD),
                new Money(75m, Currency.USD),
                stockCount: 5,
                discountedPrice: new Money(100m, Currency.USD),
                LotType.Simple,
                LotStage.Created,
                seller,
                isActive: true,
                isCompensationPaid: false,
                category: category);
        }

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category name", "Category description");
    }
}
