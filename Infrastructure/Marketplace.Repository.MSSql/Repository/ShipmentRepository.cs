using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class ShipmentRepository : IShipmentRepository
    {
        private readonly MarketplaceDbContext _db;

        public ShipmentRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Create(Shipment shipment)
        {
            if (shipment is null)
                throw new ArgumentNullException(nameof(shipment));

            var entity = Map(shipment);
            _db.Shipments.Add(entity);
            await _db.SaveChangesAsync();

            return shipment.Id;
        }

        public async Task<Shipment?> GetById(Guid shipmentId)
        {
            var entity = await _db.Shipments
                .AsNoTracking()
                .Include(s => s.ShippingAddress)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task Update(Shipment shipment)
        {
            if (shipment is null)
                throw new ArgumentNullException(nameof(shipment));

            var entity = await _db.Shipments
                .Include(s => s.ShippingAddress)
                .FirstOrDefaultAsync(s => s.Id == shipment.Id);

            if (entity is null)
                throw new KeyNotFoundException($"Shipment '{shipment.Id}' not found.");

            entity.Status = shipment.Status;
            entity.DeliveredAt = shipment.DeliveredAt;
            entity.TrackingNumber = shipment.TrackingNumber;

            entity.ShippingAddress.RecipientName = shipment.ShippingAddress.RecipientName;
            entity.ShippingAddress.Line1 = shipment.ShippingAddress.Line1;
            entity.ShippingAddress.Line2 = shipment.ShippingAddress.Line2;
            entity.ShippingAddress.City = shipment.ShippingAddress.City;
            entity.ShippingAddress.State = shipment.ShippingAddress.State;
            entity.ShippingAddress.PostalCode = shipment.ShippingAddress.PostalCode;
            entity.ShippingAddress.Country = shipment.ShippingAddress.Country;
            entity.ShippingAddress.Phone = shipment.ShippingAddress.Phone;

            await _db.SaveChangesAsync();
        }

        private static Shipment MapToDomain(ShipmentEntity entity)
        {
            var shippingAddress = new ShippingAddress(
                entity.ShippingAddress.RecipientName,
                entity.ShippingAddress.Line1,
                entity.ShippingAddress.Line2,
                entity.ShippingAddress.City,
                entity.ShippingAddress.State,
                entity.ShippingAddress.PostalCode,
                entity.ShippingAddress.Country,
                entity.ShippingAddress.Phone);

            return Shipment.Reconstruct(
                entity.Id,
                entity.OrderId,
                entity.TrackingNumber,
                entity.Status,
                shippingAddress,
                entity.Carrier,
                entity.CreatedAt,
                entity.DeliveredAt);
        }

        private static ShipmentEntity Map(Shipment shipment) =>
            new ShipmentEntity
            {
                Id = shipment.Id,
                OrderId = shipment.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                Status = shipment.Status,
                Carrier = shipment.Carrier,
                ShippingAddress = new ShippingAddressEntity
                {
                    RecipientName = shipment.ShippingAddress.RecipientName,
                    Line1 = shipment.ShippingAddress.Line1,
                    Line2 = shipment.ShippingAddress.Line2,
                    City = shipment.ShippingAddress.City,
                    State = shipment.ShippingAddress.State,
                    PostalCode = shipment.ShippingAddress.PostalCode,
                    Country = shipment.ShippingAddress.Country,
                    Phone = shipment.ShippingAddress.Phone
                },
                CreatedAt = shipment.CreatedAt,
                DeliveredAt = shipment.DeliveredAt
            };

        private IQueryable<ShipmentEntity> BuildQuery(string? search, bool onlyPending, Guid? sellerId)
        {
            var q = _db.Shipments
                .AsNoTracking()
                .Include(s => s.ShippingAddress)
                .Include(s => s.Order).ThenInclude(o => o.Items).ThenInclude(i => i.Lot)
                .Include(s => s.Order).ThenInclude(o => o.Seller)
                .AsQueryable();

            if (sellerId.HasValue)
                q = q.Where(s => s.Order.SellerId == sellerId.Value);

            if (onlyPending)
                q = q.Where(s => s.Status == ShipmentStatus.Created);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(s =>
                    s.TrackingNumber.Contains(term) ||
                    s.OrderId.ToString().Contains(term) ||
                    s.ShippingAddress.RecipientName.Contains(term) ||
                    s.ShippingAddress.Line1.Contains(term) ||
                    s.Order.Seller.Name.Contains(term));
            }

            return q;
        }

        public async Task<IEnumerable<ShipmentAdminDto>> GetAll(string? search, int page, int pageSize, bool onlyPending, Guid? sellerId)
        {
            var q = BuildQuery(search, onlyPending, sellerId);

            return await q
                .OrderByDescending(s => s.CreatedAt)
                .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
                .Take(Math.Max(pageSize, 1))
                .Select(s => new ShipmentAdminDto
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    OrderNumber = s.OrderId.ToString("N").Substring(0, 8).ToUpperInvariant(),
                    LotName = s.Order.Items.OrderBy(i => i.LotId).Select(i => i.NameSnapshot).FirstOrDefault() ?? "—",
                    LotImage = s.Order.Items.OrderBy(i => i.LotId).SelectMany(i => i.Lot.Images).Select(i => i.FilePath).FirstOrDefault() ?? string.Empty,
                    BuyerName = s.ShippingAddress.RecipientName,
                    Carrier = s.Carrier.ToString(),
                    TrackingNumber = s.TrackingNumber,
                    Address = $"{s.ShippingAddress.Line1}, {s.ShippingAddress.City}, {s.ShippingAddress.State}, {s.ShippingAddress.PostalCode}, {s.ShippingAddress.Country}",
                    RecipientName = s.ShippingAddress.RecipientName,
                    RecipientPhone = s.ShippingAddress.Phone,
                    Status = s.Status,
                    ProcessedAt = s.Status == ShipmentStatus.Created ? null : s.CreatedAt,
                    DeliveredAt = s.DeliveredAt
                })
                .ToListAsync();
        }

        public Task<int> GetTotalCount(string? search, bool onlyPending, Guid? sellerId)
            => BuildQuery(search, onlyPending, sellerId).CountAsync();

        public async Task<ShipmentStatisticsDto> GetStatistics(string? search, Guid? sellerId)
        {
            var q = BuildQuery(search, onlyPending: false, sellerId);

            return new ShipmentStatisticsDto
            {
                Total = await q.CountAsync(),
                Pending = await q.CountAsync(x => x.Status == ShipmentStatus.Created),
                InTransit = await q.CountAsync(x => x.Status == ShipmentStatus.InTransit || x.Status == ShipmentStatus.OutForDelivery || x.Status == ShipmentStatus.Shipped || x.Status == ShipmentStatus.Processing),
                Delivered = await q.CountAsync(x => x.Status == ShipmentStatus.Delivered)
            };
        }
    }
}