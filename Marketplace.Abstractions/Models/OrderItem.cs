using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Models
{
    public class OrderItem
    {
        public Guid LotId { get; }
        public string NameSnapshot { get; }
        public Money UnitPrice { get; }
        public int Quantity { get; }
        public Money Total => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);

        private OrderItem()
        {
        }

        private OrderItem(Guid lotId, string nameSnapshot, Money unitPrice, int quantity)
        {
            if (string.IsNullOrWhiteSpace(nameSnapshot))
            {
                throw new ArgumentException("Item name cannot be empty.", nameof(nameSnapshot));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            LotId = lotId;
            NameSnapshot = nameSnapshot;
            UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
            Quantity = quantity;
        }

        public static OrderItem From(PricedItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return new OrderItem(item.LotId, item.Name, item.UnitPrice, item.Quantity);
        }
    }
}
