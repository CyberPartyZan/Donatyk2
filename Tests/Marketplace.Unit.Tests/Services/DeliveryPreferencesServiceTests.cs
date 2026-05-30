using AutoFixture;
using AutoFixture.AutoMoq;
using Marketplace.Repository;
using Moq;

namespace Marketplace.Unit.Tests.Services
{
    public sealed class DeliveryPreferencesServiceTests
    {
        // ── GetByUserId ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserId_WithEmptyGuid_ThrowsArgumentException()
        {
            var fixture = CreateFixture();
            var service = fixture.Create<DeliveryPreferencesService>();

            await Assert.ThrowsAsync<ArgumentException>(() => service.GetByUserId(Guid.Empty));
        }

        [Fact]
        public async Task GetByUserId_WithValidUserId_ReturnsPreferences()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();

            var expected = new[]
            {
                DeliveryPreferences.Create(userId, DeliveryCarrier.UPS, CreateShippingAddress()),
                DeliveryPreferences.Create(userId, DeliveryCarrier.FedEx, CreateShippingAddress())
            };

            fixture.Freeze<Mock<IDeliveryPreferencesRepository>>()
                .Setup(r => r.GetByUserId(userId))
                .ReturnsAsync(expected);

            var service = fixture.Create<DeliveryPreferencesService>();

            var result = await service.GetByUserId(userId);

            Assert.Equal(2, result.Count);
        }

        // ── GetById ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_WithEmptyGuid_ThrowsArgumentException()
        {
            var fixture = CreateFixture();
            var service = fixture.Create<DeliveryPreferencesService>();

            await Assert.ThrowsAsync<ArgumentException>(() => service.GetById(Guid.Empty));
        }

        [Fact]
        public async Task GetById_WhenNotFound_ThrowsKeyNotFoundException()
        {
            var fixture = CreateFixture();
            var id = fixture.Create<Guid>();

            fixture.Freeze<Mock<IDeliveryPreferencesRepository>>()
                .Setup(r => r.GetById(id))
                .ReturnsAsync((DeliveryPreferences?)null);

            var service = fixture.Create<DeliveryPreferencesService>();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetById(id));
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsPreference()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            var preference = DeliveryPreferences.Create(userId, DeliveryCarrier.DHL, CreateShippingAddress());

            fixture.Freeze<Mock<IDeliveryPreferencesRepository>>()
                .Setup(r => r.GetById(preference.Id))
                .ReturnsAsync(preference);

            var service = fixture.Create<DeliveryPreferencesService>();

            var result = await service.GetById(preference.Id);

            Assert.Equal(preference.Id, result.Id);
            Assert.Equal(preference.Carrier, result.Carrier);
        }

        // ── GetOrCreate ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrCreate_WithEmptyUserId_ThrowsArgumentException()
        {
            var fixture = CreateFixture();
            var service = fixture.Create<DeliveryPreferencesService>();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetOrCreate(Guid.Empty, DeliveryCarrier.UPS, CreateShippingAddress()));
        }

        [Fact]
        public async Task GetOrCreate_WithNullShippingAddress_ThrowsArgumentNullException()
        {
            var fixture = CreateFixture();
            var service = fixture.Create<DeliveryPreferencesService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.GetOrCreate(fixture.Create<Guid>(), DeliveryCarrier.UPS, null!));
        }

        [Fact]
        public async Task GetOrCreate_WhenExistingMatchFound_ReturnsExistingAndDoesNotCreate()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            var address = CreateShippingAddress();
            var existing = DeliveryPreferences.Create(userId, DeliveryCarrier.UPS, address);

            var repo = fixture.Freeze<Mock<IDeliveryPreferencesRepository>>();
            repo.Setup(r => r.FindByUserCarrierAndAddress(userId, DeliveryCarrier.UPS, address))
                .ReturnsAsync(existing);

            var service = fixture.Create<DeliveryPreferencesService>();

            var result = await service.GetOrCreate(userId, DeliveryCarrier.UPS, address);

            Assert.Equal(existing.Id, result.Id);
            repo.Verify(r => r.Create(It.IsAny<DeliveryPreferences>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreate_WhenNoMatchFound_CreatesAndReturnsNewPreference()
        {
            var fixture = CreateFixture();
            var userId = fixture.Create<Guid>();
            var address = CreateShippingAddress();

            var repo = fixture.Freeze<Mock<IDeliveryPreferencesRepository>>();
            repo.Setup(r => r.FindByUserCarrierAndAddress(userId, DeliveryCarrier.FedEx, address))
                .ReturnsAsync((DeliveryPreferences?)null);

            var service = fixture.Create<DeliveryPreferencesService>();

            var result = await service.GetOrCreate(userId, DeliveryCarrier.FedEx, address);

            Assert.Equal(userId, result.UserId);
            Assert.Equal(DeliveryCarrier.FedEx, result.Carrier);
            repo.Verify(r => r.Create(It.IsAny<DeliveryPreferences>()), Times.Once);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static IFixture CreateFixture() =>
            new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private static ShippingAddress CreateShippingAddress() =>
            new("Alice", "123 Main", null, "Kyiv", "Kyivska", "01001", "Ukraine", "+380441234567");
    }
}