namespace Marketplace
{
    public class Compensation
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid LotId { get; private set; }
        public Money Amount { get; private set; }
        public CompensationStatus Status { get; private set; }

        public Compensation(Guid id, Guid orderId, Guid lotId, Money amount, CompensationStatus status)
        {
            if (id == Guid.Empty) throw new ArgumentException("Compensation id is required.", nameof(id));
            if (orderId == Guid.Empty) throw new ArgumentException("Order id is required.", nameof(orderId));
            if (lotId == Guid.Empty) throw new ArgumentException("Lot id is required.", nameof(lotId));
            if (amount is null) throw new ArgumentNullException(nameof(amount));
            if (amount.Amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            Id = id;
            OrderId = orderId;
            LotId = lotId;
            Amount = amount;
            Status = status;
        }

        public static Compensation Create(Guid orderId, Guid lotId, Money amount) =>
            new(Guid.NewGuid(), orderId, lotId, amount, CompensationStatus.Pending);

        public void MarkRequested()
        {
            if (Status == CompensationStatus.Paid)
                throw new InvalidOperationException("Cannot mark a paid compensation as requested.");

            if (Status == CompensationStatus.Pending)
                Status = CompensationStatus.Requested;
        }

        public void MarkPaid() => Status = CompensationStatus.Paid;

        public void SetStatus(CompensationStatus status) => Status = status;
    }
}