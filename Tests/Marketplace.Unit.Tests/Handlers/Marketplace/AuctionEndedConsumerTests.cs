using Marketplace.Abstractions;
using Marketplace.Payment;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Marketplace.Unit.Tests.Handlers.Marketplace
{
    public sealed class AuctionEndedConsumerTests
    {
        private readonly Mock<IOrdersRepository> _ordersRepository = new();
        private readonly Mock<ILotsRepository> _lotsRepository = new();
        private readonly Mock<IPaymentGatewayFactory> _paymentGatewayFactory = new();

        private AuctionEndedConsumer CreateConsumer() =>
            new(_ordersRepository.Object, _lotsRepository.Object, _paymentGatewayFactory.Object,
                NullLogger<AuctionEndedConsumer>.Instance);

        private static ConsumeContext<AuctionEnded> CreateContext(Guid lotId)
        {
            var context = new Mock<ConsumeContext<AuctionEnded>>();
            context.SetupGet(x => x.Message).Returns(new AuctionEnded(lotId));
            context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
            return context.Object;
        }

        [Fact]
        public async Task Consume_WhenNoPaidOrder_DoesNotCaptureOrUpdateLot()
        {
            var lotId = Guid.NewGuid();
            _ordersRepository.Setup(r => r.GetPaidOrderByLotId(lotId, CancellationToken.None))
                .ReturnsAsync((Order?)null);

            await CreateConsumer().Consume(CreateContext(lotId));

            _paymentGatewayFactory.Verify(f => f.CreatePaymentGateway(It.IsAny<string>()), Times.Never);
            _lotsRepository.Verify(r => r.UpdateLot(It.IsAny<Guid>(), It.IsAny<Lot>()), Times.Never);
        }

        [Fact]
        public async Task Consume_WhenLotNotFound_DoesNotUpdateLot()
        {
            var lotId = Guid.NewGuid();
            var order = CreateOrder(lotId);

            _ordersRepository.Setup(r => r.GetPaidOrderByLotId(lotId, CancellationToken.None))
                .ReturnsAsync(order);
            _paymentGatewayFactory.Setup(f => f.CreatePaymentGateway(order.PaymentInfo.Provider))
                .Returns(Mock.Of<IPaymentGateway>());
            _lotsRepository.Setup(r => r.GetLotById(lotId))
                .ReturnsAsync((Lot?)null);

            await CreateConsumer().Consume(CreateContext(lotId));

            _lotsRepository.Verify(r => r.UpdateLot(It.IsAny<Guid>(), It.IsAny<Lot>()), Times.Never);
        }

        [Fact]
        public async Task Consume_AfterSuccessfulCapture_SetsLotStockToZero()
        {
            var lotId = Guid.NewGuid();
            var order = CreateOrder(lotId);
            var lot = CreateAuctionLot(lotId, stockCount: 1, endOfAuction: DateTime.UtcNow.AddHours(-1));

            _ordersRepository.Setup(r => r.GetPaidOrderByLotId(lotId, CancellationToken.None))
                .ReturnsAsync(order);
            _paymentGatewayFactory.Setup(f => f.CreatePaymentGateway(order.PaymentInfo.Provider))
                .Returns(Mock.Of<IPaymentGateway>());
            _lotsRepository.Setup(r => r.GetLotById(lotId))
                .ReturnsAsync(lot);

            await CreateConsumer().Consume(CreateContext(lotId));

            Assert.Equal(0, lot.StockCount);
            _lotsRepository.Verify(r => r.UpdateLot(lotId, lot), Times.Once);
        }

        private static Order CreateOrder(Guid lotId)
        {
            var shippingInfo = new ShippingInfo(
                "Test Recipient",
                "123 Test St",
                null,
                "Test City",
                "Test State",
                "12345",
                "US",
                "+1234567890");

            var paymentInfo = new PaymentInfo(
                "TestProvider",
                0.1m,
                "https://test-return-url.com");

            var items = new List<PricedItem>
            {
                PricedItem.FromCustomPrice(
                    lotId,
                    "Test Item",
                    new Money(100m, Currency.USD),
                    1,
                    0.1m)
            };

            var order = Order.Create(Guid.NewGuid(), shippingInfo, paymentInfo, items);
            order.MarkPaid();
            return order;
        }

        private static AuctionLot CreateAuctionLot(Guid id, int stockCount, DateTime endOfAuction)
        {
            var seller = new Seller(
                Guid.NewGuid(),           
                "seller",                 
                "Test seller",            
                "seller@test.com",        
                "+1234567890",            
                null,                     
                Guid.NewGuid());          
            var category = new Category(Guid.NewGuid(), "Electronics", "Electronics category");
            var price = new Money(500m, Currency.USD);
            var compensation = new Money(300m, Currency.USD);

            return new AuctionLot(
                id, "Test Auction Lot", "Description",
                price, compensation, stockCount,
                discountedPrice: null,
                LotType.Auction, LotStage.Approved,
                seller, isActive: true, isCompensationPaid: false,
                endOfAuction: endOfAuction,
                auctionStepPercent: 10,
                category);
        }
    }
}