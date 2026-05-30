namespace Marketplace.Repository
{
    public interface IDeliveryPreferencesRepository
    {
        Task<Guid> Create(DeliveryPreferences preferences);
        Task<DeliveryPreferences?> GetById(Guid id);
        Task<IReadOnlyList<DeliveryPreferences>> GetByUserId(Guid userId);
        Task<DeliveryPreferences?> FindByUserCarrierAndAddress(Guid userId, DeliveryCarrier carrier, ShippingAddress address);
    }
}