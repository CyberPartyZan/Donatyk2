namespace Marketplace
{
    public record Ticket
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public Guid LotId { get; init; }
        public DateTime CreatedAt { get; init; }
        public bool IsWinning { get; init; }
        public bool IsPayed { get; init; }

        public Ticket(
            Guid id,
            Guid userId,
            Guid lotId,
            bool isWinning = false,
            DateTime? createdAt = null,
            bool isPayed = false)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty.", nameof(id));
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (lotId == Guid.Empty) throw new ArgumentException("LotId cannot be empty.", nameof(lotId));

            Id = id;
            UserId = userId;
            LotId = lotId;
            CreatedAt = createdAt ?? DateTime.UtcNow;
            IsWinning = isWinning;
            IsPayed = isPayed;
        }

        public static Ticket Create(Guid userId, Guid lotId) =>
            new(Guid.NewGuid(), userId, lotId, isWinning: false, createdAt: DateTime.UtcNow, isPayed: false);

        public Ticket MarkAsWinning() => this with { IsWinning = true };
        public Ticket MarkAsPayed() => this with { IsPayed = true };
    }
}