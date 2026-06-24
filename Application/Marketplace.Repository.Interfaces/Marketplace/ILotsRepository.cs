namespace Marketplace.Repository
{
    public interface ILotsRepository
    {
        Task<IEnumerable<Lot>> GetAll(LotSearchQuery query);
        Task<int> GetTotalCount(LotSearchQuery query);
        Task<Lot?> GetLotById(Guid id);
        Task<Guid> CreateLot(Lot lot);
        Task UpdateLot(Guid id, Lot lot);
        Task<IEnumerable<AuctionLot>> GetEndedAuctionLots(CancellationToken cancellationToken = default);
    }
}
