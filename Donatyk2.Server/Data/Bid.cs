using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Data
{
    public class Bid
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public LotEntity Auction { get; set; } = null!;
        public Guid BidderId { get; set; }
        public ApplicationUser Bidder { get; set; } = null!;
        public Money Amount { get; set; }
        public DateTime PlacedAt { get; set; }
    }
}
