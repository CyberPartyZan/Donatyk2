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
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task Update(Shipment shipment)
        {
            if (shipment is null)
                throw new ArgumentNullException(nameof(shipment));

            var entity = await _db.Shipments
                .FirstOrDefaultAsync(s => s.Id == shipment.Id);

            if (entity is null)
                throw new KeyNotFoundException($"Shipment '{shipment.Id}' not found.");

            entity.Status = shipment.Status;

            await _db.SaveChangesAsync();
        }

        private static Shipment MapToDomain(ShipmentEntity entity) =>
            Shipment.Reconstruct(
                entity.Id,
                entity.OrderId,
                entity.ShippingReference,
                entity.Status,
                entity.CreatedAt);

        private static ShipmentEntity Map(Shipment shipment) =>
            new ShipmentEntity
            {
                Id = shipment.Id,
                OrderId = shipment.OrderId,
                ShippingReference = shipment.TrackingNumber,
                Status = shipment.Status,
                CreatedAt = shipment.CreatedAt
            };
    }
}