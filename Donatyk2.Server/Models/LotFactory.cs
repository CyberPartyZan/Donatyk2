using Donatyk2.Server.Data;
using Donatyk2.Server.Enums;

namespace Donatyk2.Server.Models
{
    public static class LotFactory
    {
        public static Lot CreateFromEntity(LotEntity entity)
        {
            return entity.Type switch
            {
                LotType.Simple => new Lot(entity),
                LotType.Auction => new AuctionLot(entity),
                LotType.Draw => new DrawLot(entity),
                _ => new Lot(entity),
            };
        }
    }
}
