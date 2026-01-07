using Donatyk2.Server.Enums;
using Donatyk2.Server.ValueObjects;

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
            int auctionStepPercentage)
            : base(id, name, description, price, compensation, stockCount, discount, type, stage, seller, isActive, isCompensationPaid)
        {
            if (endOfAuction <= DateTime.Now)
            {
                throw new ArgumentNullException(nameof(endOfAuction), "End of auction date should be in the future.");
            }

            if (auctionStepPercentage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(auctionStepPercentage), "Auction step percent should be more that zero.");
            }

            EndOfAuction = endOfAuction;
            AuctionStepPercent = auctionStepPercentage;
        }
    }
}
