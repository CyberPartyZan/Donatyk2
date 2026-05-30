namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public sealed class DeliveryPreferencesTests
    {
        // ── Create ────────────────────────────────────────────────────────────

        [Fact]
        public void Create_WithEmptyUserId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                DeliveryPreferences.Create(Guid.Empty, DeliveryCarrier.DHL, CreateShippingAddress()));
        }

        [Fact]
        public void Create_WithNullShippingAddress_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DeliveryPreferences.Create(Guid.NewGuid(), DeliveryCarrier.UPS, null!));
        }

        [Fact]
        public void Create_WithValidArgs_SetsAllProperties()
        {
            var userId = Guid.NewGuid();
            var carrier = DeliveryCarrier.FedEx;
            var address = CreateShippingAddress();

            var pref = DeliveryPreferences.Create(userId, carrier, address);

            Assert.NotEqual(Guid.Empty, pref.Id);
            Assert.Equal(userId, pref.UserId);
            Assert.Equal(carrier, pref.Carrier);
            Assert.Same(address, pref.ShippingAddress);
        }

        [Fact]
        public void Create_CalledTwice_GeneratesDistinctIds()
        {
            var userId = Guid.NewGuid();
            var address = CreateShippingAddress();

            var p1 = DeliveryPreferences.Create(userId, DeliveryCarrier.DHL, address);
            var p2 = DeliveryPreferences.Create(userId, DeliveryCarrier.DHL, address);

            Assert.NotEqual(p1.Id, p2.Id);
        }

        // ── Reconstruct ───────────────────────────────────────────────────────

        [Fact]
        public void Reconstruct_WithEmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                DeliveryPreferences.Reconstruct(Guid.Empty, Guid.NewGuid(), DeliveryCarrier.UPS, CreateShippingAddress()));
        }

        [Fact]
        public void Reconstruct_WithEmptyUserId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                DeliveryPreferences.Reconstruct(Guid.NewGuid(), Guid.Empty, DeliveryCarrier.UPS, CreateShippingAddress()));
        }

        [Fact]
        public void Reconstruct_WithNullShippingAddress_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DeliveryPreferences.Reconstruct(Guid.NewGuid(), Guid.NewGuid(), DeliveryCarrier.DHL, null!));
        }

        [Fact]
        public void Reconstruct_WithValidArgs_SetsAllProperties()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var carrier = DeliveryCarrier.GLS;
            var address = CreateShippingAddress();

            var pref = DeliveryPreferences.Reconstruct(id, userId, carrier, address);

            Assert.Equal(id, pref.Id);
            Assert.Equal(userId, pref.UserId);
            Assert.Equal(carrier, pref.Carrier);
            Assert.Same(address, pref.ShippingAddress);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static ShippingAddress CreateShippingAddress() =>
            new("Alice", "123 Main St", null, "Kyiv", "Kyivska", "01001", "Ukraine", "+380441234567");
    }
}