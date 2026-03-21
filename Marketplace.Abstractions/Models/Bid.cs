namespace Marketplace
{
    public record Bid
    {
        public Guid Id { get; init; }
        public Guid AuctionId { get; init; }
        public Guid BidderId { get; init; }
        public Money Amount { get; init; }
        public DateTime PlacedAt { get; init; }

        public Bid(Guid id, Guid auctionId, Guid bidderId, Money amount, DateTime placedAt)
        {
            if (id == Guid.Empty) throw new ArgumentException("Bid id cannot be empty.", nameof(id));
            if (auctionId == Guid.Empty) throw new ArgumentException("Auction id cannot be empty.", nameof(auctionId));
            if (bidderId == Guid.Empty) throw new ArgumentException("Bidder id cannot be empty.", nameof(bidderId));
            if (amount.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Bid amount must be greater than zero.");

            Id = id;
            AuctionId = auctionId;
            BidderId = bidderId;
            Amount = amount;
            PlacedAt = placedAt;
        }
    }
}