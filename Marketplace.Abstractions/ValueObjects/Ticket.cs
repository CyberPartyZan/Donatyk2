namespace Marketplace
{
    public record Ticket
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public Guid LotId { get; init; }
        public bool IsWinning { get; init; }

        public Ticket(Guid id, Guid userId, Guid lotId, bool isWinning = false)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty.", nameof(id));
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (lotId == Guid.Empty) throw new ArgumentException("LotId cannot be empty.", nameof(lotId));

            Id = id;
            UserId = userId;
            LotId = lotId;
            IsWinning = isWinning;
        }

        public static Ticket Create(Guid userId, Guid lotId) =>
            new(Guid.NewGuid(), userId, lotId);

        public Ticket MarkAsWinning() => this with { IsWinning = true };
    }
}