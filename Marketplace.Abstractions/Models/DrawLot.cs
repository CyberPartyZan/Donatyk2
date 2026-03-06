namespace Marketplace
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
            Money? discountedPrice,
            LotType type,
            LotStage stage,
            Seller seller,
            bool isActive,
            bool isCompensationPaid,
            Money ticketPrice,
            string? declineReason = null,
            Category? category = null)
            : base(id, name, description, price, compensation, stockCount, discountedPrice, type, stage, seller, isActive, isCompensationPaid, declineReason, category)
        {
            if (ticketPrice is null || ticketPrice.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticketPrice), "Ticket price should be more than zero.");

            TicketPrice = ticketPrice;
        }
    }
}
