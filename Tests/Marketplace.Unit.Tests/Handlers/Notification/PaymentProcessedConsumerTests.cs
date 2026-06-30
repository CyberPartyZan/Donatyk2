using Marketplace.Abstractions;
using Marketplace.Notification;
using Marketplace.Repository;
using MassTransit;
using Moq;

namespace Marketplace.Unit.Tests.Handlers.NotificationService
{
    public sealed class PaymentProcessedConsumerTests
    {
        [Fact]
        public async Task Consume_WhenPaymentFailed_SendsFailureNotificationWithEmail()
        {
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            const string email = "buyer@example.com";

            var notificationService = new Mock<INotificationService>();
            var ordersRepository = new Mock<IOrdersRepository>();
            var usersRepository = new Mock<IUsersRepository>();

            ordersRepository.Setup(x => x.GetById(orderId)).ReturnsAsync(CreateOrder(userId));
            usersRepository.Setup(x => x.GetById(userId)).ReturnsAsync(new User(userId, email, true, false, null));

            var consumer = new PaymentProcessedConsumer(
                notificationService.Object,
                ordersRepository.Object,
                usersRepository.Object);

            var context = new Mock<ConsumeContext<PaymentProcessed>>();
            context.SetupGet(x => x.Message).Returns(new PaymentProcessed(orderId, false));

            await consumer.Consume(context.Object);

            notificationService.Verify(x => x.NotifyOrderPayFailedAsync(orderId, email), Times.Once);
            notificationService.Verify(x => x.NotifyOrderPaidAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Consume_WhenPaymentSucceeded_SendsPaidNotificationWithEmail()
        {
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            const string email = "buyer@example.com";

            var notificationService = new Mock<INotificationService>();
            var ordersRepository = new Mock<IOrdersRepository>();
            var usersRepository = new Mock<IUsersRepository>();

            ordersRepository.Setup(x => x.GetById(orderId)).ReturnsAsync(CreateOrder(userId));
            usersRepository.Setup(x => x.GetById(userId)).ReturnsAsync(new User(userId, email, true, false, null));

            var consumer = new PaymentProcessedConsumer(
                notificationService.Object,
                ordersRepository.Object,
                usersRepository.Object);

            var context = new Mock<ConsumeContext<PaymentProcessed>>();
            context.SetupGet(x => x.Message).Returns(new PaymentProcessed(orderId, true));

            await consumer.Consume(context.Object);

            notificationService.Verify(x => x.NotifyOrderPaidAsync(orderId, email), Times.Once);
            notificationService.Verify(x => x.NotifyOrderPayFailedAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        private static Order CreateOrder(Guid customerId)
        {
            var shippingInfo = new ShippingAddress("Test User", "Street 1", null, "City", "State", "00000", "US", "+10000000000");
            var paymentInfo = new PaymentInfo("Stripe", 0m, "https://example.com/return");
            var items = new List<PricedItem>
            {
                PricedItem.FromCustomPrice(Guid.NewGuid(), "Item", new Money(10m, Currency.USD), 1, 0m)
            };

            return Order.Create(customerId, CreateSeller(), shippingInfo, paymentInfo, items);
        }

        private static Seller CreateSeller() =>
            new(Guid.NewGuid(), "Seller", "Description", "seller@example.com", "+12345678901", null, Guid.NewGuid());
    }
}