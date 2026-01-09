namespace Donatyk2.Server.Data
{
    public class CartItemEntity
    {
        public Guid LotId { get; set; }
        public LotEntity Lot { get; set; } = null!;
        public int Quantity { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
