namespace Donatyk2.Server.Models
{
    public class AuctionLot : Lot
    {
        public DateTime EndOfAuction { get; set; }
        public int AuctionStepPercent { get; set; }

        public AuctionLot(Data.LotEntity entity) : base(entity)
        {
            EndOfAuction = entity.EndOfAuction ?? DateTime.Now;
            AuctionStepPercent = entity.AuctionStepPercent ?? 0;
        }
    }
}
