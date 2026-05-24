using Marketplace.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace
{
    public class TicketsService : ITicketsService
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ILotsRepository _lotsRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ClaimsPrincipal _user;

        public TicketsService(
            ClaimsPrincipal user,
            ITicketsRepository ticketsRepository,
            ILotsRepository lotsRepository,
            IOrdersRepository ordersRepository)
        {
            _user = user;
            _ticketsRepository = ticketsRepository;
            _lotsRepository = lotsRepository;
            _ordersRepository = ordersRepository;
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

        public async Task MarkAsPayedByOrderId(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Order id must be provided.", nameof(orderId));

            var order = await _ordersRepository.GetById(orderId)
                ?? throw new KeyNotFoundException($"Order '{orderId}' not found.");

            if (order.Items.Count == 0)
                return;

            var drawItemsByLot = order.Items
                .GroupBy(i => i.LotId)
                .Select(g => new { LotId = g.Key, Quantity = g.Sum(i => i.Quantity) })
                .ToList();

            var changedTickets = new List<Ticket>();

            foreach (var drawItem in drawItemsByLot)
            {
                var lot = await _lotsRepository.GetLotById(drawItem.LotId);
                if (lot is not DrawLot drawLot)
                    continue;

                var lotTickets = await _ticketsRepository.GetAll(drawLot.Id);
                drawLot.LoadTickets(lotTickets);

                var markedAsPaid = drawLot.MarkTicketsAsPayed(order.CustomerId, drawItem.Quantity);
                changedTickets.AddRange(markedAsPaid);
            }

            if (changedTickets.Count > 0)
            {
                await _ticketsRepository.Update(changedTickets.AsReadOnly());
            }
        }

        public async Task CancelTicketsForUserOnLot(Guid lotId, Guid userId, int count)
        {
            if (lotId == Guid.Empty)
                throw new ArgumentException("Lot id must be provided.", nameof(lotId));

            if (userId == Guid.Empty)
                throw new ArgumentException("User id must be provided.", nameof(userId));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

            var lot = await _lotsRepository.GetLotById(lotId);
            var drawLot = lot as DrawLot
                ?? throw new InvalidOperationException("Tickets can only be cancelled for draw lots.");

            var tickets = await _ticketsRepository.GetAll(lotId);
            drawLot.LoadTickets(tickets);

            var cancelledIds = drawLot.CancelTickets(userId, count);

            await _ticketsRepository.DeleteTickets(cancelledIds);
            await _lotsRepository.UpdateLot(lotId, drawLot);
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