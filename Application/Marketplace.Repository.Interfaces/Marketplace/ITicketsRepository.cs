namespace Marketplace.Repository
{
    public interface ITicketsRepository
    {
        Task Create(IReadOnlyCollection<Ticket> tickets);
        Task<IReadOnlyCollection<Ticket>> GetAll(Guid lotId);
        Task MarkAsWinning(Guid lotId, Guid ticketId);
        Task MarkAsPayedByOrderId(Guid orderId);
    }
}