namespace Marketplace
{
    public interface ITicketsService
    {
        Task<IReadOnlyCollection<Ticket>> GetAll(Guid lotId);
        Task<IReadOnlyCollection<Ticket>> Create(Guid lotId, int count);
        Task<Ticket> FindWinner(Guid lotId);
        Task MarkAsPayedByOrderId(Guid orderId);
    }
}