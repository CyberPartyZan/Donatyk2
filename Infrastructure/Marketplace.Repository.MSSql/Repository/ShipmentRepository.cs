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
                entity.ShippingReference,
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
                ShippingReference = shipment.TrackingNumber,
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
    }
}