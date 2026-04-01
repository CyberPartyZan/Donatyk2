namespace Marketplace.Repository.MSSql
{
    internal class BidEntity
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public virtual LotEntity Auction { get; set; } = null!;
        public Guid BidderId { get; set; }
        public virtual ApplicationUser Bidder { get; set; } = null!;
        public Money Amount { get; set; }
        public DateTime PlacedAt { get; set; }
    }
}
