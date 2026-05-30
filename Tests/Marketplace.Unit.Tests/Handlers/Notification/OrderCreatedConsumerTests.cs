using Marketplace.Abstractions;
using Marketplace.Notification;
using Marketplace.Repository;
using MassTransit;
using Moq;

namespace Marketplace.Unit.Tests.Handlers.NotificationService
{
    public sealed class OrderCreatedConsumerTests
    {
        [Fact]
        public async Task Consume_SendsOrderCreatedNotificationWithEmail()
        {
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            const string email = "buyer@example.com";

            var notificationService = new Mock<INotificationService>();
            var ordersRepository = new Mock<IOrdersRepository>();
            var usersRepository = new Mock<IUsersRepository>();

            ordersRepository.Setup(x => x.GetById(orderId)).ReturnsAsync(CreateOrder(userId));
            usersRepository.Setup(x => x.GetById(userId)).ReturnsAsync(new User(userId, email, true, false, null));

            var consumer = new OrderCreatedConsumer(
                notificationService.Object,
                ordersRepository.Object,
                usersRepository.Object);

            var context = new Mock<ConsumeContext<OrderCreated>>();
            context.SetupGet(x => x.Message).Returns(new OrderCreated(orderId, new Money(15m, Currency.USD)));

            await consumer.Consume(context.Object);

            notificationService.Verify(x => x.NotifyOrderCreatedAsync(orderId, email), Times.Once);
        }

        private static Order CreateOrder(Guid customerId)
        {
            var shippingInfo = new ShippingAddress("Test User", "Street 1", null, "City", "State", "00000", "US", "+10000000000");
            var paymentInfo = new PaymentInfo("Stripe", 0m, "https://example.com/return");
            var items = new List<PricedItem>
            {
                PricedItem.FromCustomPrice(Guid.NewGuid(), "Item", new Money(10m, Currency.USD), 1, 0m)
            };

            return Order.Create(customerId, shippingInfo, paymentInfo, items);
        }
    }
}