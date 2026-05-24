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

        public async Task Update(IReadOnlyCollection<Ticket> tickets)
        {
            ArgumentNullException.ThrowIfNull(tickets);
            if (tickets.Count == 0) return;

            var ticketById = tickets.ToDictionary(t => t.Id);
            var ticketIds = ticketById.Keys.ToList();

            var entities = await _db.Tickets
                .Where(t => ticketIds.Contains(t.Id))
                .ToListAsync();

            if (entities.Count != ticketIds.Count)
                throw new KeyNotFoundException("One or more tickets to update were not found.");

            foreach (var entity in entities)
            {
                var ticket = ticketById[entity.Id];
                entity.IsWinning = ticket.IsWinning;
                entity.IsPayed = ticket.IsPayed;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteTickets(IReadOnlyCollection<Guid> ticketIds)
        {
            ArgumentNullException.ThrowIfNull(ticketIds);
            if (ticketIds.Count == 0) return;

            var entities = await _db.Tickets
                .Where(t => ticketIds.Contains(t.Id))
                .ToListAsync();

            _db.Tickets.RemoveRange(entities);
            await _db.SaveChangesAsync();
        }
    }
}