using Donatyk2.Server.Data;
using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Models
{
    public class DrawLot : Lot
    {
        public Money TicketPrice { get; set; }

        public DrawLot(LotEntity entity) : base(entity)
        {
            if (entity.TicketPrice is null || entity.TicketPrice.Amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entity.TicketPrice), "Ticket price should be more that zero.");
            }

            TicketPrice = entity.TicketPrice;
        }
    }
}
