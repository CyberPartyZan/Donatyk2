using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Abstractions;
using Marketplace.Payment;
using Marketplace.Repository;
using MassTransit;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    /// <summary>
    /// Tests covering the DeliveryPreferences resolution paths in OrdersService
    /// (using DeliveryPreferenceId vs. inline Shipping + Carrier).
    /// </summary>
    public sealed class OrdersServiceDeliveryPreferencesTests
    {
        // ── CheckoutAsync – DeliveryPreferenceId path ────────────────────────

        [Fact]
        public async Task CheckoutAsync_WithDeliveryPreferenceId_UsesPreferenceShippingAndCarrier()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var preferenceId = Guid.NewGuid();
            var preferenceAddress = CreateShippingAddress();
            var preference = DeliveryPreferences.Reconstruct(
                preferenceId, userId, DeliveryCarrier.DHL, preferenceAddress);

            var cartLot = CreateLot(priceAmount: 100m, stockCount: 5);
            var cart = CreateCart(userId, cartLot, quantity: 1);

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(cart);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLot.Id))
                .ReturnsAsync(CreateLot(id: cartLot.Id, priceAmount: 100m, stockCount: 5));

            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Setup(s => s.GetById(preferenceId))
                .ReturnsAsync(preference);

            Order? storedOrder = null;
            fixture.Freeze<Mock<IOrdersRepository>>()
                .Setup(r => r.Create(It.IsAny<Order>()))
                .Callback<Order>(o => storedOrder = o)
                .ReturnsAsync(() => storedOrder!.Id);

            const string paymentUrl = "https://pay.test/checkout";
            var paymentGateway = fixture.Freeze<Mock<IPaymentGateway>>();
            paymentGateway.Setup(pg => pg.CreatePaymentUrlAsync(
                    It.IsAny<Order>(), It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentUrl);
            fixture.Freeze<Mock<IPaymentGatewayFactory>>()
                .Setup(f => f.CreatePaymentGateway(It.IsAny<string>()))
                .Returns(paymentGateway.Object);

            var service = fixture.Create<OrdersService>();
            var request = CreateCheckoutRequestWithPreferenceId(preferenceId);

            var response = await service.CheckoutAsync(request);

            Assert.NotNull(storedOrder);
            Assert.Equal(DeliveryCarrier.DHL, storedOrder!.DeliveryCarrier);
            Assert.Equal(preferenceAddress.RecipientName, storedOrder.ShippingAddress.RecipientName);
            Assert.Equal(preferenceAddress.Line1, storedOrder.ShippingAddress.Line1);

            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Verify(s => s.GetById(preferenceId), Times.Once);
            // GetOrCreate must NOT be called when using an existing preference
            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Verify(s => s.GetOrCreate(
                    It.IsAny<Guid>(),
                    It.IsAny<DeliveryCarrier>(),
                    It.IsAny<ShippingAddress>()), Times.Never);
        }

        [Fact]
        public async Task CheckoutAsync_WithDeliveryPreferenceId_WhenPreferenceNotFound_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var missingPreferenceId = Guid.NewGuid();

            var cartLot = CreateLot();
            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(CreateCart(userId, cartLot, quantity: 1));

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLot.Id))
                .ReturnsAsync(CreateLot(id: cartLot.Id));

            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Setup(s => s.GetById(missingPreferenceId))
                .ThrowsAsync(new KeyNotFoundException($"DeliveryPreferences '{missingPreferenceId}' not found."));

            var service = fixture.Create<OrdersService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.CheckoutAsync(CreateCheckoutRequestWithPreferenceId(missingPreferenceId)));
        }

        // ── CheckoutAsync – inline Shipping path (GetOrCreate) ───────────────

        [Fact]
        public async Task CheckoutAsync_WithInlineShipping_CallsGetOrCreateWithCorrectValues()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var cartLot = CreateLot(priceAmount: 100m, stockCount: 5);
            var cart = CreateCart(userId, cartLot, quantity: 1);

            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(cart);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLot.Id))
                .ReturnsAsync(CreateLot(id: cartLot.Id, priceAmount: 100m, stockCount: 5));

            var deliveryPrefsService = fixture.Freeze<Mock<IDeliveryPreferencesService>>();
            var createdPreference = DeliveryPreferences.Create(userId, DeliveryCarrier.FedEx, CreateShippingAddress());
            deliveryPrefsService
                .Setup(s => s.GetOrCreate(userId, DeliveryCarrier.FedEx, It.IsAny<ShippingAddress>()))
                .ReturnsAsync(createdPreference);

            Order? storedOrder = null;
            fixture.Freeze<Mock<IOrdersRepository>>()
                .Setup(r => r.Create(It.IsAny<Order>()))
                .Callback<Order>(o => storedOrder = o)
                .ReturnsAsync(() => storedOrder!.Id);

            const string paymentUrl = "https://pay.test/checkout";
            var paymentGateway = fixture.Freeze<Mock<IPaymentGateway>>();
            paymentGateway.Setup(pg => pg.CreatePaymentUrlAsync(
                    It.IsAny<Order>(), It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paymentUrl);
            fixture.Freeze<Mock<IPaymentGatewayFactory>>()
                .Setup(f => f.CreatePaymentGateway(It.IsAny<string>()))
                .Returns(paymentGateway.Object);

            var service = fixture.Create<OrdersService>();
            var request = CreateCheckoutRequestWithInlineShipping(carrier: DeliveryCarrier.FedEx);

            await service.CheckoutAsync(request);

            deliveryPrefsService.Verify(
                s => s.GetOrCreate(userId, DeliveryCarrier.FedEx, It.IsAny<ShippingAddress>()),
                Times.Once);
        }

        [Fact]
        public async Task CheckoutAsync_WithInlineShipping_PersistsOrderWithRequestCarrier()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var cartLot = CreateLot(priceAmount: 100m, stockCount: 5);
            fixture.Freeze<Mock<ICartRepository>>()
                .Setup(r => r.GetCartByUserId(userId))
                .ReturnsAsync(CreateCart(userId, cartLot, quantity: 1));

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(cartLot.Id))
                .ReturnsAsync(CreateLot(id: cartLot.Id, priceAmount: 100m, stockCount: 5));

            var createdPreference = DeliveryPreferences.Create(userId, DeliveryCarrier.USPS, CreateShippingAddress());
            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Setup(s => s.GetOrCreate(userId, DeliveryCarrier.USPS, It.IsAny<ShippingAddress>()))
                .ReturnsAsync(createdPreference);

            Order? storedOrder = null;
            fixture.Freeze<Mock<IOrdersRepository>>()
                .Setup(r => r.Create(It.IsAny<Order>()))
                .Callback<Order>(o => storedOrder = o)
                .ReturnsAsync(() => storedOrder!.Id);

            var paymentGateway = fixture.Freeze<Mock<IPaymentGateway>>();
            paymentGateway.Setup(pg => pg.CreatePaymentUrlAsync(
                    It.IsAny<Order>(), It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://pay.test/checkout");
            fixture.Freeze<Mock<IPaymentGatewayFactory>>()
                .Setup(f => f.CreatePaymentGateway(It.IsAny<string>()))
                .Returns(paymentGateway.Object);

            var service = fixture.Create<OrdersService>();

            await service.CheckoutAsync(CreateCheckoutRequestWithInlineShipping(carrier: DeliveryCarrier.USPS));

            Assert.NotNull(storedOrder);
            Assert.Equal(DeliveryCarrier.USPS, storedOrder!.DeliveryCarrier);
        }

        // ── CheckoutDrawAsync – DeliveryPreferenceId path ────────────────────

        [Fact]
        public async Task CheckoutDrawAsync_WithDeliveryPreferenceId_UsesPreferenceShippingAndCarrier()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var preferenceId = Guid.NewGuid();
            var preferenceAddress = CreateShippingAddress();
            var preference = DeliveryPreferences.Reconstruct(
                preferenceId, userId, DeliveryCarrier.GLS, preferenceAddress);

            var drawLot = CreateDrawLot(ticketPriceAmount: 5m);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(drawLot.Id))
                .ReturnsAsync(drawLot);

            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Setup(s => s.GetById(preferenceId))
                .ReturnsAsync(preference);

            fixture.Freeze<Mock<ITicketsService>>()
                .Setup(s => s.Create(drawLot.Id, 2))
                .ReturnsAsync(new[]
                {
                    Ticket.Create(userId, drawLot.Id),
                    Ticket.Create(userId, drawLot.Id)
                });

            Order? storedOrder = null;
            fixture.Freeze<Mock<IOrdersRepository>>()
                .Setup(r => r.Create(It.IsAny<Order>()))
                .Callback<Order>(o => storedOrder = o)
                .ReturnsAsync(() => storedOrder!.Id);

            var paymentGateway = fixture.Freeze<Mock<IPaymentGateway>>();
            paymentGateway.Setup(pg => pg.CreatePaymentDrawUrlAsync(
                    It.IsAny<Order>(), It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://pay.test/draw");
            fixture.Freeze<Mock<IPaymentGatewayFactory>>()
                .Setup(f => f.CreatePaymentGateway(It.IsAny<string>()))
                .Returns(paymentGateway.Object);

            var service = fixture.Create<OrdersService>();
            var request = CreateCheckoutDrawRequestWithPreferenceId(drawLot.Id, 2, preferenceId);

            await service.CheckoutDrawAsync(request);

            Assert.NotNull(storedOrder);
            Assert.Equal(DeliveryCarrier.GLS, storedOrder!.DeliveryCarrier);
            Assert.Equal(preferenceAddress.RecipientName, storedOrder.ShippingAddress.RecipientName);

            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Verify(s => s.GetById(preferenceId), Times.Once);
        }

        // ── CheckoutAuctionAsync – DeliveryPreferenceId path ─────────────────

        [Fact]
        public async Task CheckoutAuctionAsync_WithDeliveryPreferenceId_UsesPreferenceShippingAndCarrier()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            fixture.Inject(CreatePrincipal(userId));

            var preferenceId = Guid.NewGuid();
            var preferenceAddress = CreateShippingAddress();
            var preference = DeliveryPreferences.Reconstruct(
                preferenceId, userId, DeliveryCarrier.TNT, preferenceAddress);

            var auctionLot = CreateAuctionLot(startingPriceAmount: 100m);

            fixture.Freeze<Mock<ILotsRepository>>()
                .Setup(r => r.GetLotById(auctionLot.Id))
                .ReturnsAsync(auctionLot);

            fixture.Freeze<Mock<IDeliveryPreferencesService>>()
                .Setup(s => s.GetById(preferenceId))
                .ReturnsAsync(preference);

            var bidAmount = new Money(125m, Currency.USD);
            var bid = new Bid(Guid.NewGuid(), auctionLot.Id, userId, bidAmount, DateTime.UtcNow);
            fixture.Freeze<Mock<IBidsService>>()
                .Setup(s => s.PlaceBid(auctionLot.Id, bidAmount))
                .ReturnsAsync(bid);

            Order? storedOrder = null;
            fixture.Freeze<Mock<IOrdersRepository>>()
                .Setup(r => r.Create(It.IsAny<Order>()))
                .Callback<Order>(o => storedOrder = o)
                .ReturnsAsync(() => storedOrder!.Id);

            var paymentGateway = fixture.Freeze<Mock<IPaymentGateway>>();
            paymentGateway.Setup(pg => pg.CreatePaymentAuctionUrlAsync(
                    It.IsAny<Order>(), It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://pay.test/auction");
            fixture.Freeze<Mock<IPaymentGatewayFactory>>()
                .Setup(f => f.CreatePaymentGateway(It.IsAny<string>()))
                .Returns(paymentGateway.Object);

            var service = fixture.Create<OrdersService>();
            var request = CreateCheckoutAuctionRequestWithPreferenceId(auctionLot.Id, bidAmount, preferenceId);

            await service.CheckoutAuctionAsync(request);

            Assert.NotNull(storedOrder);
            Assert.Equal(DeliveryCarrier.TNT, storedOrder!.DeliveryCarrier);
            Assert.Equal(preferenceAddress.RecipientName, storedOrder.ShippingAddress.RecipientName);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ClaimsPrincipal CreatePrincipal(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));
            return new ClaimsPrincipal(identity);
        }

        private static ShippingAddress CreateShippingAddress() =>
            new("Alice", "123 Main", null, "Kyiv", "Kyivska", "01001", "Ukraine", "+380441234567");

        private static CheckoutRequest CreateCheckoutRequestWithPreferenceId(Guid preferenceId) =>
            new()
            {
                DeliveryPreferenceId = preferenceId,
                Payment = new PaymentInfoDto
                {
                    Provider = "Stripe",
                    TaxRate = 0.1m,
                    ReturnUrl = "https://example.com/return"
                }
            };

        private static CheckoutRequest CreateCheckoutRequestWithInlineShipping(
            DeliveryCarrier carrier = DeliveryCarrier.UPS) =>
            new()
            {
                Carrier = carrier,
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
                    TaxRate = 0.1m,
                    ReturnUrl = "https://example.com/return"
                }
            };

        private static CheckoutDrawRequest CreateCheckoutDrawRequestWithPreferenceId(
            Guid lotId, int ticketsCount, Guid preferenceId) =>
            new()
            {
                LotId = lotId,
                TicketsCount = ticketsCount,
                DeliveryPreferenceId = preferenceId,
                Payment = new PaymentInfoDto
                {
                    Provider = "Stripe",
                    TaxRate = 0.1m,
                    ReturnUrl = "https://example.com/return"
                }
            };

        private static CheckoutAuctionRequest CreateCheckoutAuctionRequestWithPreferenceId(
            Guid lotId, Money amount, Guid preferenceId) =>
            new()
            {
                LotId = lotId,
                Amount = amount,
                DeliveryPreferenceId = preferenceId,
                Payment = new PaymentInfoDto
                {
                    Provider = "Stripe",
                    TaxRate = 0.1m,
                    ReturnUrl = "https://example.com/return"
                }
            };

        private static Cart CreateCart(Guid userId, Lot lotSnapshot, int quantity) =>
            new Cart(new[] { new CartItem(lotSnapshot, quantity, userId) });

        private static Lot CreateLot(
            Guid? id = null,
            decimal priceAmount = 100m,
            int stockCount = 5)
        {
            var category = new Category(Guid.NewGuid(), "Cat", "Desc");
            var seller = new Seller(Guid.NewGuid(), "Seller", "Desc", "s@e.com", "+12345678901", null, Guid.NewGuid());
            return new Lot(
                id ?? Guid.NewGuid(), "Lot", "Desc",
                new Money(priceAmount, Currency.USD),
                new Money(priceAmount * 0.6m, Currency.USD),
                stockCount,
                discountedPrice: null,
                type: LotType.Simple,  // Add the missing type parameter
                stage: LotStage.Created,
                seller: seller,
                isActive: true,
                isCompensationPaid: false,
                category: category,
                declineReason: null,
                isDeleted: false);  // Also add the missing isDeleted parameter
        }

        private static DrawLot CreateDrawLot(
            Guid? id = null,
            decimal ticketPriceAmount = 10m)
        {
            var category = new Category(Guid.NewGuid(), "Cat", "Desc");
            var seller = new Seller(Guid.NewGuid(), "Seller", "Desc", "s@e.com", "+12345678901", null, Guid.NewGuid());
            return new DrawLot(
                id ?? Guid.NewGuid(), "Draw", "Desc",
                new Money(100m, Currency.USD),
                new Money(60m, Currency.USD),
                stockCount: 10,
                discountedPrice: null,
                type: LotType.Draw,
                stage: LotStage.Created,
                seller: seller,
                isActive: true,
                isCompensationPaid: false,
                ticketPrice: new Money(ticketPriceAmount, Currency.USD),
                ticketsSold: 0,
                category: category,
                declineReason: null,
                tickets: null,
                isDrawn: false,
                isDeleted: false);
        }

        private static AuctionLot CreateAuctionLot(decimal startingPriceAmount = 100m)
        {
            var category = new Category(Guid.NewGuid(), "Cat", "Desc");
            var seller = new Seller(Guid.NewGuid(), "Seller", "Desc", "s@e.com", "+12345678901", null, Guid.NewGuid());
            return new AuctionLot(
                Guid.NewGuid(), "Auction", "Desc",
                new Money(startingPriceAmount, Currency.USD),
                new Money(startingPriceAmount * 0.6m, Currency.USD),
                stockCount: 1,
                discountedPrice: null,
                type: LotType.Auction,
                stage: LotStage.Created,
                seller: seller,
                isActive: true,
                isCompensationPaid: false,
                endOfAuction: DateTime.UtcNow.AddDays(7),
                auctionStepPercent: 5,
                category: category,
                declineReason: null,
                bidHistory: null,
                isDeleted: false);
        }
    }
}