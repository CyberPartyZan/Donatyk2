using Marketplace.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Marketplace.Unit.Tests.Handlers.Marketplace
{
    public sealed class MarketplacePaymentProcessedConsumerTests
    {
        [Fact]
        public async Task Consume_WhenPaymentFailed_DoesNotMarkOrderOrTickets()
        {
            var ordersService = new Mock<IOrdersService>();
            var ticketsService = new Mock<ITicketsService>();
            var logger = NullLogger<MarketplacePaymentProcessedConsumer>.Instance;

            var consumer = new MarketplacePaymentProcessedConsumer(
                ordersService.Object,
                ticketsService.Object,
                logger);

            var context = new Mock<ConsumeContext<PaymentProcessed>>();
            context.SetupGet(x => x.Message).Returns(new PaymentProcessed(Guid.NewGuid(), false));

            await consumer.Consume(context.Object);

            ordersService.Verify(s => s.MarkPaid(It.IsAny<Guid>()), Times.Never);
            ticketsService.Verify(s => s.MarkAsPayedByOrderId(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Consume_WhenPaymentSucceeded_MarksOrderAndTicketsPaid()
        {
            var orderId = Guid.NewGuid();

            var ordersService = new Mock<IOrdersService>();
            ordersService.Setup(s => s.MarkPaid(orderId)).ReturnsAsync(Guid.NewGuid());

            var ticketsService = new Mock<ITicketsService>();
            ticketsService.Setup(s => s.MarkAsPayedByOrderId(orderId)).Returns(Task.CompletedTask);

            var logger = NullLogger<MarketplacePaymentProcessedConsumer>.Instance;

            var consumer = new MarketplacePaymentProcessedConsumer(
                ordersService.Object,
                ticketsService.Object,
                logger);

            var context = new Mock<ConsumeContext<PaymentProcessed>>();
            context.SetupGet(x => x.Message).Returns(new PaymentProcessed(orderId, true));

            await consumer.Consume(context.Object);

            ordersService.Verify(s => s.MarkPaid(orderId), Times.Once);
            ticketsService.Verify(s => s.MarkAsPayedByOrderId(orderId), Times.Once);
        }
    }
}