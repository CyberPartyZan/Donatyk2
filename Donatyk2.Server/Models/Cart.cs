namespace Donatyk2.Server.Models
{
    public class Cart
    {
        public IEnumerable<CartItem> Items { get; private set; }

        public Cart(IEnumerable<CartItem> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (items.DistinctBy(i => i.UserId).Count() != 1)
            {
                throw new ArgumentException("All cart items must belong to the same user.", nameof(items));
            }

            Items = items;
        }
    }
}
