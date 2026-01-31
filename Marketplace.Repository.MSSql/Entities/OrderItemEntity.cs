using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Data
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
