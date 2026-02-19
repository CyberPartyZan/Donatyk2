namespace Marketplace.Repository.MSSql
{
    internal class OrderItemEntity
    {
        public Guid OrderId { get; set; }
        public virtual OrderEntity Order { get; set; } = null!;
        public Guid LotId { get; set; }
        public virtual LotEntity Lot { get; set; } = null!;
        public string NameSnapshot { get; set; } = null!;
        public Money UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
