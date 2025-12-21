namespace Donatyk2.Server.Models
{
    public class DrawLot : Lot
    {
        public double TicketPrice { get; set; }

        public DrawLot(Data.LotEntity entity) : base(entity)
        {
            TicketPrice = entity.TicketPrice ?? 0;
        }
    }
}
