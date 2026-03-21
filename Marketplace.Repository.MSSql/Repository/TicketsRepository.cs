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
                IsWinning = ticket.IsWinning
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
                .Select(x => new Ticket(x.Id, x.UserId, x.LotId, x.IsWinning))
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
    }
}