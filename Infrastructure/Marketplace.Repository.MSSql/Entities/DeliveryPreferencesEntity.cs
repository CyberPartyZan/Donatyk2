namespace Marketplace.Repository.MSSql
{
    internal class DeliveryPreferencesEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DeliveryCarrier Carrier { get; set; }
        public Guid ShippingAddressId { get; set; }

        public virtual ShippingAddressEntity ShippingAddress { get; set; } = null!;
    }
}