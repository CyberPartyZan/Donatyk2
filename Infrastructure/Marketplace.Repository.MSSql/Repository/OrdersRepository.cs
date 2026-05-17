using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class OrdersRepository : IOrdersRepository
    {
        private readonly MarketplaceDbContext _db;

        public OrdersRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Create(Order order)
        {
            if (order is null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var entity = Map(order);
            _db.Orders.Add(entity);
            await _db.SaveChangesAsync();

            return order.Id;
        }

        public async Task<Order?> GetById(Guid orderId)
        {
            var entity = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (entity is null)
                return null;

            return MapToDomain(entity);
        }

        /// <summary>
        /// Returns the most recent Paid order whose items reference the given lot.
        /// This is the winning bid hold order that needs to be captured.
        /// </summary>
        public async Task<Order?> GetPaidOrderByLotId(Guid lotId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .Where(o => o.Status == OrderStatus.Paid
                            && o.Items.Any(i => i.LotId == lotId))
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task<Guid> MarkPaid(Guid orderId)
        {
            var entity = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (entity is null)
                throw new KeyNotFoundException($"Order '{orderId}' not found.");

            if (entity.Status == OrderStatus.Paid)
                return entity.CustomerId;

            entity.Status = OrderStatus.Paid;
            await _db.SaveChangesAsync();

            return entity.CustomerId;
        }

        public async Task<Guid> MarkPaid(Guid orderId, string provider, string paymentReference)
        {
            var entity = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (entity is null)
                throw new KeyNotFoundException($"Order '{orderId}' not found.");

            if (!string.Equals(entity.PaymentProvider, provider, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Payment provider does not match the order payment provider.");

            if (entity.Status == OrderStatus.Paid)
                return entity.CustomerId;

            entity.Status = OrderStatus.Paid;
            entity.PaymentReference = paymentReference;
            await _db.SaveChangesAsync();

            return entity.CustomerId;
        }

        public async Task Cancel(Guid orderId)
        {
            var entity = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (entity is null)
                throw new KeyNotFoundException($"Order '{orderId}' not found.");

            if (entity.Status == OrderStatus.Cancelled)
                return;

            if (entity.Status != OrderStatus.Created)
                throw new InvalidOperationException($"Only created orders can be cancelled. Current status: {entity.Status}.");

            entity.Status = OrderStatus.Cancelled;
            await _db.SaveChangesAsync();
        }

        private static Order MapToDomain(OrderEntity entity)
        {
            var shippingInfo = new ShippingInfo(
                entity.ShippingRecipientName,
                entity.ShippingLine1,
                entity.ShippingLine2,
                entity.ShippingCity,
                entity.ShippingState,
                entity.ShippingPostalCode,
                entity.ShippingCountry,
                entity.ShippingPhone);

            var paymentInfo = new PaymentInfo(
                entity.PaymentProvider,
                entity.PaymentTaxRate,
                entity.PaymentReturnUrl);

            var pricedItems = entity.Items
                .Select(i => PricedItem.FromCustomPrice(i.LotId, i.NameSnapshot, i.UnitPrice, i.Quantity, 0m))
                .ToList();

            return Order.Create(entity.CustomerId, shippingInfo, paymentInfo, pricedItems);
        }

        private static OrderEntity Map(Order order)
        {
            return new OrderEntity
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                ShippingRecipientName = order.ShippingInfo.RecipientName,
                ShippingLine1 = order.ShippingInfo.Line1,
                ShippingLine2 = order.ShippingInfo.Line2,
                ShippingCity = order.ShippingInfo.City,
                ShippingState = order.ShippingInfo.State,
                ShippingPostalCode = order.ShippingInfo.PostalCode,
                ShippingCountry = order.ShippingInfo.Country,
                ShippingPhone = order.ShippingInfo.Phone,
                PaymentProvider = order.PaymentInfo.Provider,
                PaymentTaxRate = order.PaymentInfo.TaxRate,
                PaymentReturnUrl = order.PaymentInfo.ReturnUrl,
                PaymentReference = order.PaymentInfo.Reference,
                Items = order.Items
                    .Select(item => new OrderItemEntity
                    {
                        OrderId = order.Id,
                        LotId = item.LotId,
                        NameSnapshot = item.NameSnapshot,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity
                    })
                    .ToList()
            };
        }
    }
}