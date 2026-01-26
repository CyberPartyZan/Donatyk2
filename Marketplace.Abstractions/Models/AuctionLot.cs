using Donatyk2.Server.ValueObjects;
using Donatyk2.Server.Enums;

namespace Donatyk2.Server.Models
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
            double discount,
            LotType type,
            LotStage stage,
            Seller seller,
            bool isActive,
            bool isCompensationPaid,
            DateTime endOfAuction,
            int auctionStepPercent)
            : base(id, name, description, price, compensation, stockCount, discount, type, stage, seller, isActive, isCompensationPaid)
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
