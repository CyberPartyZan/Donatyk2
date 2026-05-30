namespace Marketplace
{
    public class DeliveryPreferences
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DeliveryCarrier Carrier { get; private set; }
        public ShippingAddress ShippingAddress { get; private set; } = null!;

        private DeliveryPreferences() { }

        public static DeliveryPreferences Create(Guid userId, DeliveryCarrier carrier, ShippingAddress shippingAddress)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId must be provided.", nameof(userId));

            if (shippingAddress is null)
                throw new ArgumentNullException(nameof(shippingAddress));

            return new DeliveryPreferences
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Carrier = carrier,
                ShippingAddress = shippingAddress
            };
        }

        public static DeliveryPreferences Reconstruct(
            Guid id,
            Guid userId,
            DeliveryCarrier carrier,
            ShippingAddress shippingAddress)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be provided.", nameof(id));

            if (userId == Guid.Empty)
                throw new ArgumentException("UserId must be provided.", nameof(userId));

            if (shippingAddress is null)
                throw new ArgumentNullException(nameof(shippingAddress));

            return new DeliveryPreferences
            {
                Id = id,
                UserId = userId,
                Carrier = carrier,
                ShippingAddress = shippingAddress
            };
        }
    }
}