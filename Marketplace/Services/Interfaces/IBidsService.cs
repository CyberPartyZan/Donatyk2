namespace Marketplace
{
    public interface IBidsService
    {
        Task<Bid> PlaceBid(Guid lotId, Money amount);
        Task<IReadOnlyCollection<Bid>> LoadBidHistory(Guid lotId);
    }
}