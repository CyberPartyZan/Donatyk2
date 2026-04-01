namespace Marketplace.Repository
{
    public interface IBidsRepository
    {
        Task PlaceBid(Bid bid);
        Task<IReadOnlyCollection<Bid>> LoadBidHistory(Guid lotId);
    }
}