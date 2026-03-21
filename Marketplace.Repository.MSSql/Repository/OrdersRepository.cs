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

        // TODO: Provider should be enum
        public async Task<Guid> MarkPaid(Guid orderId, string provider, string paymentReference)
        {
            var entity = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (entity is null)
            {
                throw new KeyNotFoundException($"Order '{orderId}' not found.");
            }

            if (!string.Equals(entity.PaymentProvider, provider, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Payment provider does not match the order payment provider.");
            }

            if (entity.Status == OrderStatus.Paid)
            {
                return entity.CustomerId;
            }

            entity.Status = OrderStatus.Paid;
            entity.PaymentReference = paymentReference;
            await _db.SaveChangesAsync();

            return entity.CustomerId;
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