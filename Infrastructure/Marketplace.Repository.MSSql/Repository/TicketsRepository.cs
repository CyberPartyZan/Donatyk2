using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class TicketsRepository : ITicketsRepository
    {
        private readonly MarketplaceDbContext _db;

        public TicketsRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task Create(IReadOnlyCollection<Ticket> tickets)
        {
            ArgumentNullException.ThrowIfNull(tickets);
            if (tickets.Count == 0) return;

            var entities = tickets.Select(ticket => new TicketEntity
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                LotId = ticket.LotId,
                CreatedAt = ticket.CreatedAt,
                IsWinning = ticket.IsWinning,
                IsPayed = ticket.IsPayed
            });

            _db.Tickets.AddRange(entities);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<Ticket>> GetAll(Guid lotId)
        {
            var entities = await _db.Tickets
                .AsNoTracking()
                .Where(x => x.LotId == lotId)
                .ToListAsync();

            return entities
                .Select(x => new Ticket(
                    x.Id,
                    x.UserId,
                    x.LotId,
                    x.IsWinning,
                    x.CreatedAt,
                    x.IsPayed))
                .ToList();
        }

        public async Task MarkAsWinning(Guid lotId, Guid ticketId)
        {
            var entities = await _db.Tickets
                .Where(x => x.LotId == lotId)
                .ToListAsync();

            if (entities.Count == 0)
                throw new InvalidOperationException("No tickets found for this lot.");

            var hasWinner = false;
            foreach (var ticket in entities)
            {
                ticket.IsWinning = ticket.Id == ticketId;
                if (ticket.IsWinning) hasWinner = true;
            }

            if (!hasWinner)
                throw new KeyNotFoundException($"Winner ticket '{ticketId}' was not found for lot '{lotId}'.");

            await _db.SaveChangesAsync();
        }

        public async Task MarkAsPayedByOrderId(Guid orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .SingleOrDefaultAsync(o => o.Id == orderId);

            if (order is null)
                throw new KeyNotFoundException($"Order '{orderId}' not found.");

            if (order.Items.Count == 0)
                return;

            var lotIds = order.Items.Select(i => i.LotId).Distinct().ToList();

            var lotTypes = await _db.Lots
                .Where(l => lotIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Type })
                .ToDictionaryAsync(x => x.Id, x => x.Type);

            var drawItems = order.Items
                .Where(i => lotTypes.TryGetValue(i.LotId, out var type) && type == LotType.Draw)
                .ToList();

            if (drawItems.Count == 0)
                return;

            foreach (var item in drawItems)
            {
                var ticketsToMark = await _db.Tickets
                    .Where(t => t.LotId == item.LotId && t.UserId == order.CustomerId && !t.IsPayed)
                    .OrderBy(t => t.CreatedAt)
                    .Take(item.Quantity)
                    .ToListAsync();

                if (ticketsToMark.Count < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Not enough unpaid tickets to mark as paid for lot '{item.LotId}' and user '{order.CustomerId}'.");
                }

                foreach (var ticket in ticketsToMark)
                {
                    ticket.IsPayed = true;
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}