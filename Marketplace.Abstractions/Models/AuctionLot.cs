namespace Marketplace
{
    public class AuctionLot : Lot
    {
        public DateTime EndOfAuction { get; set; }
        public int AuctionStepPercent { get; set; }

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
            string? declineReason = null,
            Category? category = null)
            : base(id, name, description, price, compensation, stockCount, discountedPrice, type, stage, seller, isActive, isCompensationPaid, declineReason, category)
        {
            if (endOfAuction <= DateTime.UtcNow)
                throw new ArgumentException("End of auction date should be in the future.", nameof(endOfAuction));

            if (auctionStepPercent <= 0)
                throw new ArgumentOutOfRangeException(nameof(auctionStepPercent), "Auction step percent should be greater than zero.");

            EndOfAuction = endOfAuction;
            AuctionStepPercent = auctionStepPercent;
        }
    }
}
