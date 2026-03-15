namespace Marketplace
{
    public class DrawLot : Lot
    {
        public Money TicketPrice { get; set; }
        public int TicketsSold { get; set; }
        public int TotalTickets => CalculateTotalTickets(Price, TicketPrice);
        public int TicketsLeft => Math.Max(0, TotalTickets - TicketsSold);

        public DrawLot(
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
            Money ticketPrice,
            int ticketsSold = 0,
            Category category = null!,
            string? declineReason = null)
            : base(id, name, description, price, compensation, stockCount, discountedPrice, type, stage, seller, isActive, isCompensationPaid, category, declineReason)
        {
            if (ticketPrice is null || ticketPrice.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticketPrice), "Ticket price should be more than zero.");

            if (ticketsSold < 0)
                throw new ArgumentOutOfRangeException(nameof(ticketsSold), "Tickets sold cannot be negative.");

            var totalTickets = CalculateTotalTickets(price, ticketPrice);
            if (ticketsSold > totalTickets)
                throw new ArgumentOutOfRangeException(nameof(ticketsSold), "Tickets sold cannot exceed total tickets.");

            TicketPrice = ticketPrice;
            TicketsSold = ticketsSold;
        }

        private static int CalculateTotalTickets(Money price, Money ticketPrice)
        {
            if (ticketPrice.Amount <= 0)
            {
                return 0;
            }

            return (int)decimal.Floor(price.Amount / ticketPrice.Amount);
        }
    }
}
