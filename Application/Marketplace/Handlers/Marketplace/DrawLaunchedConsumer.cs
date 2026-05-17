using Marketplace.Abstractions;
using Marketplace.Notification;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    internal class DrawLaunchedConsumer : IConsumer<DrawLaunched>
    {
        private readonly ITicketsService _ticketsService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DrawLaunchedConsumer> _logger;

        public DrawLaunchedConsumer(
            ITicketsService ticketsService,
            INotificationService notificationService,
            ILogger<DrawLaunchedConsumer> logger)
        {
            _ticketsService = ticketsService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DrawLaunched> context)
        {
            var lotId = context.Message.LotId;

            _logger.LogInformation("DrawLaunched received for lot {LotId}. Picking winner.", lotId);

            var winner = await _ticketsService.FindWinner(lotId);

            await _notificationService.NotifyDrawWinnerAsync(winner.UserId, lotId, winner.Id);

            _logger.LogInformation(
                "Winner picked for lot {LotId}. Winning ticket {TicketId} belongs to user {UserId}.",
                lotId, winner.Id, winner.UserId);
        }
    }
}