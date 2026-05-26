using Marketplace.Abstractions;
using Marketplace.Notification;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    internal class DrawLaunchedConsumer : IConsumer<DrawLaunched>
    {
        private readonly ITicketsService _ticketsService;
        private readonly INotificationService _notificationService;
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<DrawLaunchedConsumer> _logger;

        public DrawLaunchedConsumer(
            ITicketsService ticketsService,
            INotificationService notificationService,
            IUsersRepository usersRepository,
            ILogger<DrawLaunchedConsumer> logger)
        {
            _ticketsService = ticketsService;
            _notificationService = notificationService;
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DrawLaunched> context)
        {
            var lotId = context.Message.LotId;

            _logger.LogInformation("DrawLaunched received for lot {LotId}. Picking winner.", lotId);

            var winner = await _ticketsService.FindWinner(lotId);
            var user = await _usersRepository.GetById(winner.UserId)
                ?? throw new KeyNotFoundException($"User '{winner.UserId}' not found.");

            await _notificationService.NotifyDrawWinnerAsync(winner.UserId, user.Email, lotId, winner.Id);

            _logger.LogInformation(
                "Winner picked for lot {LotId}. Winning ticket {TicketId} belongs to user {UserId}.",
                lotId, winner.Id, winner.UserId);
        }
    }
}