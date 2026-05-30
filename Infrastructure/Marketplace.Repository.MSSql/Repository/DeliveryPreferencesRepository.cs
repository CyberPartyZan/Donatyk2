using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class DeliveryPreferencesRepository : IDeliveryPreferencesRepository
    {
        private readonly MarketplaceDbContext _db;

        public DeliveryPreferencesRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Create(DeliveryPreferences preferences)
        {
            if (preferences is null)
                throw new ArgumentNullException(nameof(preferences));

            var entity = Map(preferences);
            _db.DeliveryPreferences.Add(entity);
            await _db.SaveChangesAsync();

            return preferences.Id;
        }

        public async Task<DeliveryPreferences?> GetById(Guid id)
        {
            var entity = await _db.DeliveryPreferences
                .AsNoTracking()
                .Include(x => x.ShippingAddress)
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task<IReadOnlyList<DeliveryPreferences>> GetByUserId(Guid userId)
        {
            var entities = await _db.DeliveryPreferences
                .AsNoTracking()
                .Include(x => x.ShippingAddress)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<DeliveryPreferences?> FindByUserCarrierAndAddress(
            Guid userId, DeliveryCarrier carrier, ShippingAddress address)
        {
            var entity = await _db.DeliveryPreferences
                .AsNoTracking()
                .Include(x => x.ShippingAddress)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Carrier == carrier &&
                    x.ShippingAddress.RecipientName == address.RecipientName &&
                    x.ShippingAddress.Line1 == address.Line1 &&
                    x.ShippingAddress.Line2 == address.Line2 &&
                    x.ShippingAddress.City == address.City &&
                    x.ShippingAddress.State == address.State &&
                    x.ShippingAddress.PostalCode == address.PostalCode &&
                    x.ShippingAddress.Country == address.Country &&
                    x.ShippingAddress.Phone == address.Phone);

            return entity is null ? null : MapToDomain(entity);
        }

        private static DeliveryPreferences MapToDomain(DeliveryPreferencesEntity entity) =>
            DeliveryPreferences.Reconstruct(
                entity.Id,
                entity.UserId,
                entity.Carrier,
                new ShippingAddress(
                    entity.ShippingAddress.RecipientName,
                    entity.ShippingAddress.Line1,
                    entity.ShippingAddress.Line2,
                    entity.ShippingAddress.City,
                    entity.ShippingAddress.State,
                    entity.ShippingAddress.PostalCode,
                    entity.ShippingAddress.Country,
                    entity.ShippingAddress.Phone));

        private static DeliveryPreferencesEntity Map(DeliveryPreferences p) =>
            new DeliveryPreferencesEntity
            {
                Id = p.Id,
                UserId = p.UserId,
                Carrier = p.Carrier,
                ShippingAddress = new ShippingAddressEntity
                {
                    Id = Guid.NewGuid(),
                    RecipientName = p.ShippingAddress.RecipientName,
                    Line1 = p.ShippingAddress.Line1,
                    Line2 = p.ShippingAddress.Line2,
                    City = p.ShippingAddress.City,
                    State = p.ShippingAddress.State,
                    PostalCode = p.ShippingAddress.PostalCode,
                    Country = p.ShippingAddress.Country,
                    Phone = p.ShippingAddress.Phone
                }
            };
    }
}