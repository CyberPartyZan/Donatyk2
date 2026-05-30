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
                throw new ArgumentNullException(nameof(order));

            var entity = Map(order);
            _db.Orders.Add(entity);
            await _db.SaveChangesAsync();

            return order.Id;
        }

        public async Task<Order?> GetById(Guid orderId)
        {
            var entity = await _db.Orders
                .AsNoTracking()
                .Include(o => o.ShippingAddress)
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
                .Include(o => o.ShippingAddress)
                .Include(o => o.Items)
                .Where(o => o.Status == OrderStatus.Paid
                            && o.Items.Any(i => i.LotId == lotId))
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task Update(Order order)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

            var entity = await _db.Orders
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (entity is null)
                throw new KeyNotFoundException($"Order '{order.Id}' not found.");

            entity.Status = order.Status;
            entity.Total = order.Total;
            entity.PaymentProvider = order.PaymentInfo.Provider;
            entity.PaymentTaxRate = order.PaymentInfo.TaxRate;
            entity.PaymentReturnUrl = order.PaymentInfo.ReturnUrl;
            entity.PaymentReference = order.PaymentInfo.Reference;
            entity.ShipmentId = order.ShipmentId;
            entity.DeliveryCarrier = order.DeliveryCarrier;

            entity.ShippingAddress.RecipientName = order.ShippingAddress.RecipientName;
            entity.ShippingAddress.Line1 = order.ShippingAddress.Line1;
            entity.ShippingAddress.Line2 = order.ShippingAddress.Line2;
            entity.ShippingAddress.City = order.ShippingAddress.City;
            entity.ShippingAddress.State = order.ShippingAddress.State;
            entity.ShippingAddress.PostalCode = order.ShippingAddress.PostalCode;
            entity.ShippingAddress.Country = order.ShippingAddress.Country;
            entity.ShippingAddress.Phone = order.ShippingAddress.Phone;

            await _db.SaveChangesAsync();
        }

        private static Order MapToDomain(OrderEntity entity)
        {
            var shippingInfo = new ShippingAddress(
                entity.ShippingAddress.RecipientName,
                entity.ShippingAddress.Line1,
                entity.ShippingAddress.Line2,
                entity.ShippingAddress.City,
                entity.ShippingAddress.State,
                entity.ShippingAddress.PostalCode,
                entity.ShippingAddress.Country,
                entity.ShippingAddress.Phone);

            var paymentInfo = new PaymentInfo(
                entity.PaymentProvider,
                entity.PaymentTaxRate,
                entity.PaymentReturnUrl,
                entity.PaymentReference);

            var pricedItems = entity.Items
                .Select(i => PricedItem.FromCustomPrice(i.LotId, i.NameSnapshot, i.UnitPrice, i.Quantity, 0m))
                .ToList();

            return Order.Reconstruct(
                entity.Id,
                entity.CustomerId,
                entity.Status,
                entity.CreatedAt,
                shippingInfo,
                paymentInfo,
                pricedItems,
                entity.ShipmentId,
                entity.DeliveryCarrier);
        }

        private static OrderEntity Map(Order order) =>
            new OrderEntity
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                ShipmentId = order.ShipmentId,
                DeliveryCarrier = order.DeliveryCarrier,
                ShippingAddress = new ShippingAddressEntity
                {
                    RecipientName = order.ShippingAddress.RecipientName,
                    Line1 = order.ShippingAddress.Line1,
                    Line2 = order.ShippingAddress.Line2,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    PostalCode = order.ShippingAddress.PostalCode,
                    Country = order.ShippingAddress.Country,
                    Phone = order.ShippingAddress.Phone
                },
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