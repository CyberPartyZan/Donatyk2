using Marketplace.Abstractions;
using Marketplace.Notification;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Marketplace.Unit.Tests.Handlers.Marketplace
{
    public sealed class DrawLaunchedConsumerTests
    {
        [Fact]
        public async Task Consume_WhenWinnerFound_SendsWinnerNotificationWithEmail()
        {
            var lotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            const string email = "winner@example.com";

            var ticketsService = new Mock<ITicketsService>();
            var notificationService = new Mock<INotificationService>();
            var usersRepository = new Mock<IUsersRepository>();

            ticketsService.Setup(x => x.FindWinner(lotId))
                .ReturnsAsync(new Ticket(ticketId, userId, lotId, isWinning: true));

            usersRepository.Setup(x => x.GetById(userId))
                .ReturnsAsync(new User(userId, email, true, false, null));

            var consumer = new DrawLaunchedConsumer(
                ticketsService.Object,
                notificationService.Object,
                usersRepository.Object,
                NullLogger<DrawLaunchedConsumer>.Instance);

            var context = new Mock<ConsumeContext<DrawLaunched>>();
            context.SetupGet(x => x.Message).Returns(new DrawLaunched(lotId));

            await consumer.Consume(context.Object);

            notificationService.Verify(x => x.NotifyDrawWinnerAsync(userId, email, lotId, ticketId), Times.Once);
        }
    }
}