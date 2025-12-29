using Donatyk2.Server.Data;

namespace Donatyk2.Server.Models
{
    public class AuctionLot : Lot
    {
        public DateTime EndOfAuction { get; set; }
        public int AuctionStepPercent { get; set; }

        public AuctionLot(LotEntity entity) : base(entity)
        {
            if (entity.EndOfAuction is null || entity.EndOfAuction <= DateTime.Now)
            {
                throw new ArgumentNullException(nameof(entity.EndOfAuction), "End of auction date should be in the future.");
            }

            if (entity.AuctionStepPercent is null || entity.AuctionStepPercent <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entity.AuctionStepPercent), "Auction step percent should be more that zero.");
            }

            EndOfAuction = entity.EndOfAuction.Value;
            AuctionStepPercent = entity.AuctionStepPercent.Value;
        }
    }
}
