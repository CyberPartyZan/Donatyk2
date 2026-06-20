namespace Marketplace.Repository.MSSql
{
    internal class CompensationEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid LotId { get; set; }
        public Money Amount { get; set; } = null!;
        public CompensationStatus Status { get; set; }

        public virtual OrderEntity Order { get; set; } = null!;
        public virtual LotEntity Lot { get; set; } = null!;
    }
}