using Donatyk2.Server.ValueObjects;
using Donatyk2.Server.Enums;

namespace Donatyk2.Server.Models
{
    public class DrawLot : Lot
    {
        public Money TicketPrice { get; set; }

        public DrawLot(
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
            Money ticketPrice)
            : base(id, name, description, price, compensation, stockCount, discount, type, stage, seller, isActive, isCompensationPaid)
        {
            if (ticketPrice is null || ticketPrice.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticketPrice), "Ticket price should be more than zero.");

            TicketPrice = ticketPrice;
        }
    }
}
