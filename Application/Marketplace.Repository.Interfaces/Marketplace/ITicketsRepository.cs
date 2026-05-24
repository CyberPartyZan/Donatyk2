namespace Marketplace.Repository
{
    public interface ITicketsRepository
    {
        Task Create(IReadOnlyCollection<Ticket> tickets);
        Task<IReadOnlyCollection<Ticket>> GetAll(Guid lotId);
        Task Update(IReadOnlyCollection<Ticket> tickets);
        Task DeleteTickets(IReadOnlyCollection<Guid> ticketIds);
    }
}