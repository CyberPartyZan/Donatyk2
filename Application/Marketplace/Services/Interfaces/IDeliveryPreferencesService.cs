namespace Marketplace
{
    public interface IDeliveryPreferencesService
    {
        Task<IReadOnlyList<DeliveryPreferences>> GetByUserId(Guid userId);
        Task<DeliveryPreferences> GetById(Guid id);
        Task<DeliveryPreferences> GetOrCreate(Guid userId, DeliveryCarrier carrier, ShippingAddress shippingAddress);
    }
}