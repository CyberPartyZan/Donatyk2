using Donatyk2.Server.Enums;
using Donatyk2.Server.ValueObjects;

namespace Donatyk2.Server.Models
{
    public class Order
    {
        private readonly List<OrderItem> _items = new();

        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public Money Total { get; private set; } = null!;
        public ShippingInfo ShippingInfo { get; private set; } = null!;
        public PaymentInfo PaymentInfo { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        private Order()
        {
        }

        public static Order Create(
            Guid userId,
            ShippingInfo shippingInfo,
            PaymentInfo paymentInfo,
            IReadOnlyList<PricedItem> items)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("UserId must be provided.", nameof(userId));
            }

            if (shippingInfo is null)
            {
                throw new ArgumentNullException(nameof(shippingInfo));
            }

            if (paymentInfo is null)
            {
                throw new ArgumentNullException(nameof(paymentInfo));
            }

            if (items is null || items.Count == 0)
            {
                throw new ArgumentException("At least one item is required to create an order.", nameof(items));
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = userId,
                Status = OrderStatus.Created,
                ShippingInfo = shippingInfo,
                PaymentInfo = paymentInfo,
                CreatedAt = DateTime.UtcNow
            };

            var firstCurrency = items[0].UnitPrice.Currency;
            var runningTotal = new Money(0m, firstCurrency);

            foreach (var pricedItem in items)
            {
                if (pricedItem.UnitPrice.Currency != firstCurrency)
                {
                    throw new InvalidOperationException("All items in the order must use the same currency.");
                }

                var orderItem = OrderItem.From(pricedItem);
                order._items.Add(orderItem);
                runningTotal += orderItem.Total;
            }

            order.Total = runningTotal;

            return order;
        }

        public void MarkPaid()
        {
            if (Status != OrderStatus.Created)
            {
                throw new InvalidOperationException("Only created orders can be marked as paid.");
            }

            Status = OrderStatus.Paid;
        }
    }
}
