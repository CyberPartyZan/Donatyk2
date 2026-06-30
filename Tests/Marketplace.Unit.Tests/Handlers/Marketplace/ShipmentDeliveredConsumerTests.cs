using Marketplace.Abstractions;
using Marketplace.Repository;
using MassTransit;
using Moq;

namespace Marketplace.Unit.Tests.Handlers.Marketplace
{
    public sealed class ShipmentDeliveredConsumerTests
    {
        [Fact]
        public async Task Consume_MarksOrderAsCompleted()
        {
            var orderId = Guid.NewGuid();
            var shipmentId = Guid.NewGuid();
            var deliveredAt = DateTime.UtcNow;

            var order = CreatePaidOrder(orderId);
            var ordersRepository = new Mock<IOrdersRepository>();
            ordersRepository.Setup(x => x.GetById(orderId)).ReturnsAsync(order);

            var consumer = new ShipmentDeliveredConsumer(ordersRepository.Object);

            var context = new Mock<ConsumeContext<ShipmentDelivered>>();
            context.SetupGet(x => x.Message).Returns(new ShipmentDelivered(orderId, shipmentId, deliveredAt));

            await consumer.Consume(context.Object);

            Assert.Equal(OrderStatus.Completed, order.Status);
            ordersRepository.Verify(x => x.Update(order), Times.Once);
        }

        [Fact]
        public async Task Consume_ThrowsWhenOrderNotFound()
        {
            var orderId = Guid.NewGuid();
            var ordersRepository = new Mock<IOrdersRepository>();
            ordersRepository.Setup(x => x.GetById(orderId)).ReturnsAsync((Order?)null);

            var consumer = new ShipmentDeliveredConsumer(ordersRepository.Object);

            var context = new Mock<ConsumeContext<ShipmentDelivered>>();
            context.SetupGet(x => x.Message)
                   .Returns(new ShipmentDelivered(orderId, Guid.NewGuid(), DateTime.UtcNow));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => consumer.Consume(context.Object));
        }

        private static Order CreatePaidOrder(Guid orderId)
        {
            var shippingInfo = new ShippingAddress("Test User", "Street 1", null, "City", "State", "00000", "US", "+10000000000");
            var paymentInfo = new PaymentInfo("Stripe", 0m, "https://example.com/return");
            var items = new List<PricedItem>
            {
                PricedItem.FromCustomPrice(Guid.NewGuid(), "Item", new Money(10m, Currency.USD), 1, 0m)
            };

            var order = Order.Create(orderId, CreateSeller(), shippingInfo, paymentInfo, items);
            order.AttachShipment(Guid.NewGuid());
            order.MarkPaid();
            return order;
        }

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller", "Description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}