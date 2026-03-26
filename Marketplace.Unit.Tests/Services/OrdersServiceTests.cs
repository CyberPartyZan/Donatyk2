using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class OrdersServiceTests
    {
        [Fact]
        public async Task CheckoutAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CheckoutAsync(null!));
        }

        [Fact]
        public async Task CheckoutAsync_WithoutUserIdClaim_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            fixture.Inject(new ClaimsPrincipal(new ClaimsIdentity()));

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync(CreateCheckoutRequest()));
        }

        [Fact]
        public async Task CheckoutAsync_WithEmptyCart_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(new Cart(Array.Empty<CartItem>()));

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync(CreateCheckoutRequest()));
        }

        [Fact]
        public async Task CheckoutAsync_WhenLotMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var cartLot = CreateLot();
            var cart = CreateCart(userId, cartLot, quantity: 1);

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(cart);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLot.Id))
                .ReturnsAsync((Lot?)null);

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CheckoutAsync(CreateCheckoutRequest()));
        }

        [Fact]
        public async Task CheckoutAsync_WhenStockInsufficient_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var cartLot = CreateLot(stockCount: 5);
            var cart = CreateCart(userId, cartLot, quantity: 5);

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(cart);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLot.Id))
                .ReturnsAsync(CreateLot(id: cartLot.Id, stockCount: 4));

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync(CreateCheckoutRequest()));
        }

        [Fact]
        public async Task CheckoutAsync_WhenPriceChanged_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var cartLot = CreateLot(priceAmount: 100m);
            var cart = CreateCart(userId, cartLot, quantity: 1);

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(cart);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLot.Id))
                .ReturnsAsync(CreateLot(id: cartLot.Id, priceAmount: 200m));

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync(CreateCheckoutRequest()));
        }

        [Fact]
        public async Task CheckoutAsync_WhenDrawLotInCart_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var drawLot = CreateDrawLot();
            var cart = CreateCart(userId, drawLot, quantity: 1);

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(cart);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(drawLot.Id))
                .ReturnsAsync(drawLot);

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync(CreateCheckoutRequest()));
        }

        [Fact]
        public async Task CheckoutAsync_WhenAuctionLotInCart_ThrowsInvalidOperationException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var auctionLot = CreateAuctionLot();
            var cart = CreateCart(userId, auctionLot, quantity: 1);

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(cart);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(auctionLot.Id))
                .ReturnsAsync(auctionLot);

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync(CreateCheckoutRequest()));
        }

        [Fact]
        public async Task CheckoutAsync_WithValidCart_ReturnsPaymentResponseAndClearsCart()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            const int quantity = 2;
            var cartLotSnapshot = CreateLot(priceAmount: 150m, stockCount: 10);
            var cart = CreateCart(userId, cartLotSnapshot, quantity);

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            cartRepository.Setup(r => r.GetCartByUserId(userId)).ReturnsAsync(cart);

            var liveLot = CreateLot(id: cartLotSnapshot.Id, priceAmount: 150m, stockCount: 10);
            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLotSnapshot.Id))
                .ReturnsAsync(liveLot);

            Order? storedOrder = null;
            var ordersRepository = fixture.Freeze<Mock<IOrdersRepository>>();
            ordersRepository.Setup(r => r.Create(It.IsAny<Order>()))
                .Callback<Order>(o => storedOrder = o)
                .ReturnsAsync(() => storedOrder!.Id);

            PaymentInfo? gatewayPaymentInfo = null;
            const string paymentUrl = "https://pay.test/checkout";
            fixture.Freeze<Mock<IPaymentGateway>>()
                .Setup(pg => pg.CreatePaymentUrlAsync(
                    It.IsAny<Order>(),
                    It.IsAny<PaymentInfo>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Order, PaymentInfo, CancellationToken>((o, p, _) => gatewayPaymentInfo = p)
                .ReturnsAsync(paymentUrl);

            var service = fixture.Create<OrdersService>();
            var request = CreateCheckoutRequest();

            var response = await service.CheckoutAsync(request);

            Assert.NotNull(storedOrder);
            Assert.Equal(storedOrder!.Id, response.OrderId);
            Assert.Equal(paymentUrl, response.PaymentUrl);
            Assert.Equal(request.Payment.Provider, gatewayPaymentInfo!.Provider);
            ordersRepository.Verify(r => r.Create(It.IsAny<Order>()), Times.Once);
            cartRepository.Verify(r => r.ClearCart(userId), Times.Once);
        }

        [Fact]
        public async Task HandlePaymentWebhookAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.HandlePaymentWebhookAsync(null!));
        }

        [Fact]
        public async Task HandlePaymentWebhookAsync_WhenPaymentFailed_DoesNotMarkPaid()
        {
            var fixture = CreateFixture();
            var request = new PaymentWebhookRequest
            {
                OrderId = Guid.NewGuid(),
                Provider = "Stripe",
                Reference = "ref-1",
                IsSuccess = false
            };

            var ordersRepository = fixture.Freeze<Mock<IOrdersRepository>>();
            var service = fixture.Create<OrdersService>();

            await service.HandlePaymentWebhookAsync(request);

            ordersRepository.Verify(r => r.MarkPaid(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ClaimsPrincipal CreatePrincipal(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));
            return new ClaimsPrincipal(identity);
        }

        private static CheckoutRequest CreateCheckoutRequest() =>
            new()
            {
                Shipping = new ShippingInfoDto
                {
                    RecipientName = "Alice",
                    Line1 = "123 Main",
                    City = "Kyiv",
                    State = "Kyivska",
                    PostalCode = "01001",
                    Country = "Ukraine",
                    Phone = "+380441234567"
                },
                Payment = new PaymentInfoDto
                {
                    Provider = "Stripe",
                    TaxRate = 0.2m,
                    ReturnUrl = "https://example.com/return"
                }
            };

        private static Cart CreateCart(Guid userId, Lot lotSnapshot, int quantity)
        {
            var cartItem = new CartItem(lotSnapshot, quantity, userId);
            return new Cart(new[] { cartItem });
        }

        private static Lot CreateLot(
            Guid? id = null,
            decimal priceAmount = 100m,
            decimal compensationAmount = 60m,
            int stockCount = 5,
            LotStage stage = LotStage.Created,
            LotType type = LotType.Simple,
            Guid? sellerUserId = null,
            string? declineReason = null)
        {
            var category = CreateCategory();

            return new Lot(
                id ?? Guid.NewGuid(),
                "Lot " + Guid.NewGuid().ToString("N"),
                "Desc " + Guid.NewGuid().ToString("N"),
                CreateMoney(priceAmount),
                CreateMoney(compensationAmount),
                stockCount,
                discountedPrice: null,
                type,
                stage,
                CreateSeller(sellerUserId),
                isActive: true,
                isCompensationPaid: false,
                category: category,
                declineReason: declineReason);
        }

        private static DrawLot CreateDrawLot(
            Guid? id = null,
            decimal priceAmount = 100m,
            decimal ticketPriceAmount = 10m,
            decimal compensationAmount = 60m,
            int stockCount = 5,
            LotStage stage = LotStage.Created,
            LotType type = LotType.Draw,
            Guid? sellerUserId = null,
            string? declineReason = null)
        {
            var category = CreateCategory();

            return new DrawLot(
                id ?? Guid.NewGuid(),
                "Draw " + Guid.NewGuid().ToString("N"),
                "Desc " + Guid.NewGuid().ToString("N"),
                CreateMoney(priceAmount),
                CreateMoney(compensationAmount),
                stockCount,
                discountedPrice: null,
                type,
                stage,
                CreateSeller(sellerUserId),
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: CreateMoney(ticketPriceAmount),
                category: category,
                declineReason: declineReason,
                tickets: null,
                isDrawn: false,
                isDeleted: false);
        }

        private static AuctionLot CreateAuctionLot(
            Guid? id = null,
            decimal startingPriceAmount = 100m,
            decimal compensationAmount = 60m,
            decimal? instantBuyPriceAmount = null,
            int stockCount = 5,
            LotStage stage = LotStage.Created,
            Guid? sellerUserId = null,
            string? declineReason = null)
        {
            var category = CreateCategory();

            return new AuctionLot(
                id ?? Guid.NewGuid(),
                "Auction " + Guid.NewGuid().ToString("N"),
                "Desc " + Guid.NewGuid().ToString("N"),
                CreateMoney(startingPriceAmount),
                CreateMoney(compensationAmount),
                stockCount,
                instantBuyPriceAmount.HasValue ? CreateMoney(instantBuyPriceAmount.Value) : null,
                LotType.Auction,
                stage,
                CreateSeller(sellerUserId),
                isActive: true,
                isCompensationPaid: false,
                endOfAuction: DateTime.UtcNow.AddDays(7),
                auctionStepPercent: 5,
                category: category,
                declineReason: declineReason,
                bidHistory: null,
                isDeleted: false);
        }

        private static Category CreateCategory() =>
            new(Guid.NewGuid(), "Category name", "Category description");

        private static Seller CreateSeller(Guid? userId = null) =>
            new(Guid.NewGuid(), "Seller", "Description", "seller@example.com", "+12345678901", null, userId ?? Guid.NewGuid());

        private static Money CreateMoney(decimal amount) => new(amount, Currency.USD);

        [Fact]
        public async Task CheckoutDrawAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            fixture.Inject(CreatePrincipal(fixture.Create<Guid>()));

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CheckoutDrawAsync(null!));
        }

        [Fact]
        public async Task CheckoutDrawAsync_WhenLotMissing_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var lotId = fixture.Create<Guid>();
            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(lotId))
                .ReturnsAsync((Lot?)null);

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CheckoutDrawAsync(CreateCheckoutDrawRequest(lotId, 2)));
        }

        [Fact]
        public async Task CheckoutDrawAsync_WithValidRequest_CreatesTicketsAndReturnsPaymentResponse()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var drawLot = CreateDrawLot(ticketPriceAmount: 5m);
            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(drawLot.Id))
                .ReturnsAsync(drawLot);

            var ticketsService = fixture.Freeze<Mock<ITicketsService>>();
            ticketsService
                .Setup(s => s.Create(drawLot.Id, 3))
                .ReturnsAsync(new[]
                {
                    Ticket.Create(userId, drawLot.Id),
                    Ticket.Create(userId, drawLot.Id),
                    Ticket.Create(userId, drawLot.Id)
                });

            Order? storedOrder = null;
            var ordersRepository = fixture.Freeze<Mock<IOrdersRepository>>();
            ordersRepository.Setup(r => r.Create(It.IsAny<Order>()))
                .Callback<Order>(o => storedOrder = o)
                .ReturnsAsync(() => storedOrder!.Id);

            const string paymentUrl = "https://pay.test/draw-checkout";
            fixture.Freeze<Mock<IPaymentGateway>>()
                .Setup(pg => pg.CreatePaymentUrlAsync(
                    It.IsAny<Order>(),
                    It.IsAny<PaymentInfo>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentUrl);

            var cartRepository = fixture.Freeze<Mock<ICartRepository>>();
            var service = fixture.Create<OrdersService>();

            var response = await service.CheckoutDrawAsync(CreateCheckoutDrawRequest(drawLot.Id, 3));

            Assert.NotNull(storedOrder);
            Assert.Equal(storedOrder!.Id, response.OrderId);
            Assert.Equal(paymentUrl, response.PaymentUrl);
            Assert.Single(storedOrder.Items);
            Assert.Equal(drawLot.Id, storedOrder.Items.Single().LotId);
            Assert.Equal(3, storedOrder.Items.Single().Quantity);

            ticketsService.Verify(s => s.Create(drawLot.Id, 3), Times.Once);
            ordersRepository.Verify(r => r.Create(It.IsAny<Order>()), Times.Once);
            cartRepository.Verify(r => r.ClearCart(It.IsAny<Guid>()), Times.Never);
        }

        private static CheckoutDrawRequest CreateCheckoutDrawRequest(Guid lotId, int ticketsCount) =>
            new()
            {
                LotId = lotId,
                TicketsCount = ticketsCount,
                Shipping = new ShippingInfoDto
                {
                    RecipientName = "Alice",
                    Line1 = "123 Main",
                    City = "Kyiv",
                    State = "Kyivska",
                    PostalCode = "01001",
                    Country = "Ukraine",
                    Phone = "+380441234567"
                },
                Payment = new PaymentInfoDto
                {
                    Provider = "Stripe",
                    TaxRate = 0.2m,
                    ReturnUrl = "https://example.com/return"
                }
            };
    }
}