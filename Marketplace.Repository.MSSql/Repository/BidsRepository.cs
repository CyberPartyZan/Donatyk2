using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class BidsRepository : IBidsRepository
    {
        private readonly MarketplaceDbContext _db;

        public BidsRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task PlaceBid(Bid bid)
        {
            ArgumentNullException.ThrowIfNull(bid);

            var lot = await _db.Lots.FirstOrDefaultAsync(l => l.Id == bid.AuctionId && !l.IsDeleted);
            if (lot is null)
                throw new KeyNotFoundException($"Auction lot '{bid.AuctionId}' not found.");
            if (lot.Type != LotType.Auction)
                throw new InvalidOperationException("Bids can be placed only for auction lots.");

            _db.BidHistory.Add(new BidEntity
            {
                Id = bid.Id,
                AuctionId = bid.AuctionId,
                BidderId = bid.BidderId,
                Amount = bid.Amount,
                PlacedAt = bid.PlacedAt
            });

            lot.Price = bid.Amount;
            _db.Lots.Update(lot);

            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<Bid>> LoadBidHistory(Guid lotId)
        {
            var bids = await _db.BidHistory
                .AsNoTracking()
                .Where(b => b.AuctionId == lotId)
                .OrderBy(b => b.PlacedAt)
                .ToListAsync();

            return bids
                .Select(b => new Bid(b.Id, b.AuctionId, b.BidderId, b.Amount, b.PlacedAt))
                .ToList();
        }
    }
}