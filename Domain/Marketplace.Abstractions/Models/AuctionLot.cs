namespace Marketplace
{
    public class AuctionLot : Lot
    {
        public DateTime EndOfAuction { get; set; }
        public int AuctionStepPercent { get; set; }
        public IReadOnlyCollection<Bid> BidHistory { get; private set; }

        public AuctionLot(
            Guid id,
            string name,
            string description,
            Money price,
            Money compensation,
            int stockCount,
            Money? discountedPrice,
            LotType type,
            LotStage stage,
            Seller seller,
            bool isActive,
            bool isCompensationPaid,
            DateTime endOfAuction,
            int auctionStepPercent,
            Category category,
            string? declineReason = null,
            IReadOnlyCollection<Bid>? bidHistory = null,
            bool isDeleted = false)
            : base(id, name, description, price, compensation, stockCount, discountedPrice, type, stage, seller, isActive, isCompensationPaid, category, declineReason, isDeleted)
        {
            if (endOfAuction <= DateTime.UtcNow)
                throw new ArgumentException("End of auction date should be in the future.", nameof(endOfAuction));
            if (auctionStepPercent <= 0)
                throw new ArgumentOutOfRangeException(nameof(auctionStepPercent), "Auction step percent should be greater than zero.");

            EndOfAuction = endOfAuction;
            AuctionStepPercent = auctionStepPercent;
            BidHistory = bidHistory ?? Array.Empty<Bid>();
        }

        public void LoadBidHistory(IReadOnlyCollection<Bid> history)
        {
            BidHistory = history ?? throw new ArgumentNullException(nameof(history));
        }

        public Bid Bid(Guid bidderId, Money amount)
        {
            if (EndOfAuction <= DateTime.UtcNow)
                throw new InvalidOperationException("Auction is closed.");
            if (amount.Currency != Price.Currency)
                throw new InvalidOperationException("Bid currency must match lot currency.");
            if (amount <= Price)
                throw new InvalidOperationException("Bid amount must be greater than current lot price.");

            var bid = new Bid(Guid.NewGuid(), Id, bidderId, amount, DateTime.UtcNow);
            BidHistory = BidHistory.Concat(new[] { bid }).ToList().AsReadOnly();
            Price = amount;

            return bid;
        }

        public override void Sell(int quantity)
        {
            if (EndOfAuction > DateTime.UtcNow)
                throw new InvalidOperationException("Auction lot can't be sold until the end of auction.");

            base.Sell(quantity);
        }

        public override void Delete()
        {
            if (BidHistory.Any())
                throw new InvalidOperationException("Auction lot cannot be deleted when there is bid history.");

            base.Delete();
        }
    }
}
