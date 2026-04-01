namespace Marketplace.Repository.MSSql
{
    internal class CartItemEntity
    {
        public Guid LotId { get; set; }
        public virtual LotEntity Lot { get; set; } = null!;
        public int Quantity { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
