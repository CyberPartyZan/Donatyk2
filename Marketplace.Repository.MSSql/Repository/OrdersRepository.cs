using Donatyk2.Server.Data;
using Donatyk2.Server.Enums;
using Donatyk2.Server.Models;
using Donatyk2.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Donatyk2.Server.Repositories
{
    internal class OrdersRepository : IOrdersRepository
    {
        private readonly DonatykDbContext _db;

        public OrdersRepository(DonatykDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Create(Order order)
        {
            if (order is null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();

            foreach (var item in order.Items)
            {
                var lot = await _db.Lots.FirstOrDefaultAsync(l => l.Id == item.LotId);

                if (lot is null)
                {
                    throw new KeyNotFoundException($"Lot with id '{item.LotId}' not found.");
                }

                if (lot.StockCount < item.Quantity)
                {
                    throw new InvalidOperationException($"Lot '{lot.Name}' does not have enough stock to fulfill the order.");
                }

                if (lot.Type == LotType.Auction)
                {
                    lot.Price = item.UnitPrice;

                    var bid = new BidEntity
                    {
                        Id = Guid.NewGuid(),
                        AuctionId = lot.Id,
                        BidderId = order.CustomerId,
                        Amount = item.UnitPrice,
                        PlacedAt = DateTime.UtcNow
                    };

                    _db.BidHistory.Add(bid);
                }

                lot.StockCount -= item.Quantity;
                _db.Lots.Update(lot);
            }

            var entity = Map(order);
            _db.Orders.Add(entity);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

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