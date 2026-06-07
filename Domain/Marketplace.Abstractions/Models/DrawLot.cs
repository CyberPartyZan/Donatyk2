namespace Marketplace
{
    public class DrawLot : Lot
    {
        public Money TicketPrice { get; private set; }
        public int TicketsSold { get; private set; }
        public int TotalTickets => CalculateTotalTickets(Price, TicketPrice);
        public int TicketsLeft => Math.Max(0, TotalTickets - TicketsSold);

        public IReadOnlyCollection<Ticket>? Tickets { get; private set; }
        public bool IsDrawn { get; private set; }

        public bool ReadyToDraw =>
            Tickets is not null &&
            TicketsSold == TotalTickets &&
            Tickets.Count == TicketsSold &&
            Tickets.All(t => t.IsPayed);

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
            bool isDrawn = false,
            bool isDeleted = false,
            Characteristic[]? characteristics = null,
            Image[]? images = null)
            : base(id, name, description, price, compensation, stockCount, discountedPrice, type, stage, seller, isActive, isCompensationPaid, category, declineReason, isDeleted, characteristics, images)
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

        /// <summary>
        /// Cancels unpaid tickets for the given user, removing them from the lot and decrementing TicketsSold.
        /// Tickets must be loaded via <see cref="LoadTickets"/> before calling this method.
        /// </summary>
        /// <returns>The IDs of the tickets that were cancelled and must be deleted by the repository.</returns>
        public IReadOnlyCollection<Guid> CancelTickets(Guid userId, int count)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

            if (Tickets is null)
                throw new InvalidOperationException("Tickets must be loaded before cancelling.");

            if (IsDrawn)
                throw new InvalidOperationException("Cannot cancel tickets after the draw has completed.");

            var toCancel = Tickets
                .Where(t => t.UserId == userId && !t.IsPayed)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToList();

            if (toCancel.Count < count)
                throw new InvalidOperationException(
                    $"Not enough unpaid tickets to cancel for user '{userId}'. Requested: {count}, available: {toCancel.Count}.");

            var cancelledIds = toCancel.Select(t => t.Id).ToHashSet();

            Tickets = Tickets
                .Where(t => !cancelledIds.Contains(t.Id))
                .ToList()
                .AsReadOnly();

            TicketsSold -= toCancel.Count;

            return cancelledIds.ToList().AsReadOnly();
        }

        public Ticket FindWinner()
        {
            if (Tickets is null)
                throw new InvalidOperationException("Tickets must be loaded before finding a winner.");

            if (Tickets.Count == 0)
                throw new InvalidOperationException("No tickets available.");

            if (!ReadyToDraw)
                throw new InvalidOperationException("Winner can be found only when all tickets are sold and paid.");

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

        public override void Sell(int quantity)
        {
            if (TicketsSold != TotalTickets)
                throw new InvalidOperationException("Draw lot can be sold only when all tickets are sold.");

            if (Tickets is null || !Tickets.Any(t => t.IsWinning))
                throw new InvalidOperationException("Draw lot can be sold only when a winner ticket is present.");

            base.Sell(quantity);
        }

        public override void Delete()
        {
            if (TicketsSold > 0)
                throw new InvalidOperationException("Draw lot cannot be deleted when tickets were sold.");

            base.Delete();
        }

        public IReadOnlyCollection<Ticket> MarkTicketsAsPayed(Guid userId, int count)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

            if (Tickets is null)
                throw new InvalidOperationException("Tickets must be loaded before marking as paid.");

            var toMark = Tickets
                .Where(t => t.UserId == userId && !t.IsPayed)
                .OrderBy(t => t.CreatedAt)
                .Take(count)
                .ToList();

            if (toMark.Count < count)
                throw new InvalidOperationException(
                    $"Not enough unpaid tickets to mark as paid for user '{userId}'. Requested: {count}, available: {toMark.Count}.");

            var idsToMark = toMark.Select(t => t.Id).ToHashSet();

            var updatedTickets = Tickets
                .Select(t => idsToMark.Contains(t.Id) ? t.MarkAsPayed() : t)
                .ToList()
                .AsReadOnly();

            Tickets = updatedTickets;

            return updatedTickets
                .Where(t => idsToMark.Contains(t.Id))
                .ToList()
                .AsReadOnly();
        }

        private static int CalculateTotalTickets(Money price, Money ticketPrice)
        {
            if (ticketPrice.Amount <= 0) return 0;
            return (int)decimal.Floor(price.Amount / ticketPrice.Amount);
        }
    }
}
