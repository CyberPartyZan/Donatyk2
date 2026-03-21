namespace Marketplace
{
    public class DrawLot : Lot
    {
        public Money TicketPrice { get; set; }
        public int TicketsSold { get; set; }
        public int TotalTickets => CalculateTotalTickets(Price, TicketPrice);
        public int TicketsLeft => Math.Max(0, TotalTickets - TicketsSold);

        public IReadOnlyCollection<Ticket>? Tickets { get; private set; }
        public bool IsDrawn { get; private set; }

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
            string? declineReason = null,
            IReadOnlyCollection<Ticket>? tickets = null,
            bool isDrawn = false)
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
            Tickets = tickets;
            IsDrawn = isDrawn;
        }

        public void LoadTickets(IReadOnlyCollection<Ticket> tickets)
        {
            Tickets = tickets ?? throw new ArgumentNullException(nameof(tickets));
        }

        public IReadOnlyCollection<Ticket> ProduceTickets(Guid userId, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

            if (IsDrawn)
                throw new InvalidOperationException("Draw already completed.");

            if (TicketsSold + count > TotalTickets)
                throw new InvalidOperationException("Not enough tickets left.");

            var produced = Enumerable.Range(0, count)
                .Select(_ => Ticket.Create(userId, Id))
                .ToList();

            TicketsSold += count;

            if (Tickets is not null)
            {
                Tickets = Tickets.Concat(produced).ToList().AsReadOnly();
            }

            return produced.AsReadOnly();
        }

        public Ticket FindWinner()
        {
            if (Tickets is null)
                throw new InvalidOperationException("Tickets must be loaded before finding a winner.");

            if (TicketsSold != TotalTickets)
                throw new InvalidOperationException("Winner can be found only when all tickets are sold.");

            if (Tickets.Count != TicketsSold)
                throw new InvalidOperationException("Loaded tickets count must match TicketsSold.");

            if (Tickets.Count == 0)
                throw new InvalidOperationException("No tickets available.");

            if (IsDrawn)
            {
                var existingWinner = Tickets.FirstOrDefault(t => t.IsWinning);
                if (existingWinner is not null) return existingWinner;
                throw new InvalidOperationException("Draw is already completed but winner ticket is missing.");
            }

            var normalized = Tickets.Select(t => t with { IsWinning = false }).ToList();
            var winnerIndex = Random.Shared.Next(normalized.Count);
            var winner = normalized[winnerIndex].MarkAsWinning();
            normalized[winnerIndex] = winner;

            Tickets = normalized.AsReadOnly();
            IsDrawn = true;

            return winner;
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
