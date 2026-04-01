namespace Marketplace
{
    public class Cart
    {
        public IReadOnlyCollection<CartItem> Items { get; }

        public Cart(IEnumerable<CartItem> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var materializedItems = items.ToList();

            if (materializedItems.Count > 0 &&
                materializedItems.DistinctBy(i => i.UserId).Count() != 1)
            {
                throw new ArgumentException("All cart items must belong to the same user.", nameof(items));
            }

            Items = materializedItems;
        }
    }
}
