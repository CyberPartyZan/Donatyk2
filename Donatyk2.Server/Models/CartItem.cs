namespace Donatyk2.Server.Models
{
    public class CartItem
    {
        public Lot Lot { get; private set; }
        public int Quantity { get; private set; }
        public Guid UserId { get; private set; }

        public CartItem(Lot lot, int quantity, Guid userId)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("UserId must be a valid GUID.", nameof(userId));
            }

            Lot = lot ?? throw new ArgumentNullException(nameof(lot), "Lot cannot be null.");
            Quantity = quantity;
            UserId = userId;
        }
    }
}
