using Marketplace.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace
{
    public class TicketsService : ITicketsService
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ILotsRepository _lotsRepository;
        private readonly ClaimsPrincipal _user;

        public TicketsService(
            ClaimsPrincipal user,
            ITicketsRepository ticketsRepository,
            ILotsRepository lotsRepository)
        {
            _user = user;
            _ticketsRepository = ticketsRepository;
            _lotsRepository = lotsRepository;
        }

        public Task<IReadOnlyCollection<Ticket>> GetAll(Guid lotId)
        {
            return _ticketsRepository.GetAll(lotId);
        }

        public async Task<IReadOnlyCollection<Ticket>> Create(Guid lotId, int count)
        {
            var lot = await _lotsRepository.GetLotById(lotId);
            var drawLot = lot as DrawLot ?? throw new InvalidOperationException("Tickets can be created only for draw lots.");

            var userId = GetCurrentUserIdOrThrow();
            var newTickets = drawLot.ProduceTickets(userId, count);

            await _ticketsRepository.Create(newTickets);
            await _lotsRepository.UpdateLot(lotId, drawLot);

            return newTickets;
        }

        public async Task<Ticket> FindWinner(Guid lotId)
        {
            var lot = await _lotsRepository.GetLotById(lotId);
            var drawLot = lot as DrawLot ?? throw new InvalidOperationException("Winner can be selected only for draw lots.");

            var tickets = await _ticketsRepository.GetAll(lotId);
            drawLot.LoadTickets(tickets);

            var winner = drawLot.FindWinner();

            await _ticketsRepository.MarkAsWinning(lotId, winner.Id);
            await _lotsRepository.UpdateLot(lotId, drawLot);

            return winner;
        }

        public Task MarkAsPayedByOrderId(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Order id must be provided.", nameof(orderId));

            return _ticketsRepository.MarkAsPayedByOrderId(orderId);
        }

        private Guid GetCurrentUserIdOrThrow()
        {
            var userIdValue =
                _user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                _user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userIdValue))
                throw new InvalidOperationException("User id is not available in the current principal.");

            return Guid.Parse(userIdValue);
        }
    }
}