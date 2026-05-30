using Marketplace.Repository;

namespace Marketplace
{
    public class DeliveryPreferencesService : IDeliveryPreferencesService
    {
        private readonly IDeliveryPreferencesRepository _repository;

        public DeliveryPreferencesService(IDeliveryPreferencesRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<DeliveryPreferences>> GetByUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId must be provided.", nameof(userId));

            return await _repository.GetByUserId(userId);
        }

        public async Task<DeliveryPreferences> GetById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be provided.", nameof(id));

            return await _repository.GetById(id)
                ?? throw new KeyNotFoundException($"DeliveryPreferences '{id}' not found.");
        }

        public async Task<DeliveryPreferences> GetOrCreate(Guid userId, DeliveryCarrier carrier, ShippingAddress shippingAddress)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId must be provided.", nameof(userId));

            if (shippingAddress is null)
                throw new ArgumentNullException(nameof(shippingAddress));

            var existing = await _repository.FindByUserCarrierAndAddress(userId, carrier, shippingAddress);
            if (existing is not null)
                return existing;

            var preferences = DeliveryPreferences.Create(userId, carrier, shippingAddress);
            await _repository.Create(preferences);

            return preferences;
        }
    }
}