namespace Marketplace
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
        public Guid? ShipmentId { get; private set; }
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        private Order() { }

        /// <summary>
        /// Reconstructs an <see cref="Order"/> from persisted data, preserving the original Id,
        /// Status, CreatedAt, and all other fields as stored in the database.
        /// </summary>
        public static Order Reconstruct(
            Guid id,
            Guid customerId,
            OrderStatus status,
            DateTime createdAt,
            ShippingInfo shippingInfo,
            PaymentInfo paymentInfo,
            IReadOnlyList<PricedItem> items,
            Guid? shipmentId = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be provided.", nameof(id));

            if (customerId == Guid.Empty)
                throw new ArgumentException("CustomerId must be provided.", nameof(customerId));

            if (shippingInfo is null)
                throw new ArgumentNullException(nameof(shippingInfo));

            if (paymentInfo is null)
                throw new ArgumentNullException(nameof(paymentInfo));

            if (items is null || items.Count == 0)
                throw new ArgumentException("At least one item is required.", nameof(items));

            var order = new Order
            {
                Id = id,
                CustomerId = customerId,
                Status = status,
                CreatedAt = createdAt,
                ShippingInfo = shippingInfo,
                PaymentInfo = paymentInfo,
                ShipmentId = shipmentId
            };

            var firstCurrency = items[0].UnitPrice.Currency;
            var runningTotal = new Money(0m, firstCurrency);

            foreach (var pricedItem in items)
            {
                if (pricedItem.UnitPrice.Currency != firstCurrency)
                    throw new InvalidOperationException("All items in the order must use the same currency.");

                var orderItem = OrderItem.From(pricedItem);
                order._items.Add(orderItem);
                runningTotal += orderItem.Total;
            }

            order.Total = runningTotal;

            return order;
        }

        public static Order Create(
            Guid userId,
            ShippingInfo shippingInfo,
            PaymentInfo paymentInfo,
            IReadOnlyList<PricedItem> items)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId must be provided.", nameof(userId));

            if (shippingInfo is null)
                throw new ArgumentNullException(nameof(shippingInfo));

            if (paymentInfo is null)
                throw new ArgumentNullException(nameof(paymentInfo));

            if (items is null || items.Count == 0)
                throw new ArgumentException("At least one item is required to create an order.", nameof(items));

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
                    throw new InvalidOperationException("All items in the order must use the same currency.");

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
                throw new InvalidOperationException("Only created orders can be marked as paid.");

            Status = OrderStatus.Paid;
        }

        public void AttachShipment(Guid shipmentId)
        {
            if (shipmentId == Guid.Empty)
                throw new ArgumentException("ShipmentInfoId must be provided.", nameof(shipmentId));

            if (ShipmentId.HasValue)
                throw new InvalidOperationException("A shipment is already attached to this order.");

            ShipmentId = shipmentId;
        }

        public void MarkProcessing()
        {
            if (Status != OrderStatus.Paid)
                throw new InvalidOperationException("Only paid orders can be moved to processing.");

            Status = OrderStatus.Processing;
        }

        public void MarkShipped()
        {
            if (Status != OrderStatus.Processing)
                throw new InvalidOperationException("Only processing orders can be marked as shipped.");

            Status = OrderStatus.Shipped;
        }

        public void MarkInTransit()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException("Only shipped orders can be marked as in transit.");

            Status = OrderStatus.InTransit;
        }

        public void MarkOutForDelivery()
        {
            if (Status != OrderStatus.InTransit)
                throw new InvalidOperationException("Only in-transit orders can be marked as out for delivery.");

            Status = OrderStatus.OutForDelivery;
        }

        public void MarkDelivered()
        {
            if (Status != OrderStatus.OutForDelivery)
                throw new InvalidOperationException("Only out-for-delivery orders can be marked as delivered.");

            Status = OrderStatus.Delivered;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Order is already cancelled.");

            if (Status == OrderStatus.Completed)
                throw new InvalidOperationException("Completed orders cannot be cancelled.");

            Status = OrderStatus.Cancelled;
        }
    }
}
