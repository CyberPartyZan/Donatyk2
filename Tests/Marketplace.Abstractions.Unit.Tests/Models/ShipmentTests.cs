using Xunit;

namespace Marketplace.Abstractions.Unit.Tests.Models
{
    public class ShipmentTests
    {
        private static Shipment CreateValidShipment() =>
            Shipment.Create(Guid.NewGuid(), "TRACK-001");

        [Fact]
        public void Create_WithValidArgs_SetsCreatedStatus()
        {
            var shipment = CreateValidShipment();

            Assert.Equal(ShipmentStatus.Created, shipment.Status);
        }

        [Fact]
        public void Create_WithEmptyOrderId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                Shipment.Create(Guid.Empty, "TRACK-001"));
        }

        [Fact]
        public void Create_WithBlankReference_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                Shipment.Create(Guid.NewGuid(), "   "));
        }

        [Fact]
        public void TakeIntoProcessing_FromCreated_Succeeds()
        {
            var shipment = CreateValidShipment();
            shipment.TakeIntoProcessing();

            Assert.Equal(ShipmentStatus.Processing, shipment.Status);
        }

        [Fact]
        public void TakeIntoProcessing_WhenNotCreated_Throws()
        {
            var shipment = CreateValidShipment();
            shipment.TakeIntoProcessing();

            Assert.Throws<InvalidOperationException>(() => shipment.TakeIntoProcessing());
        }

        [Fact]
        public void MarkShipped_FromProcessing_Succeeds()
        {
            var shipment = CreateValidShipment();
            shipment.TakeIntoProcessing();
            shipment.MarkShipped();

            Assert.Equal(ShipmentStatus.Shipped, shipment.Status);
        }

        [Fact]
        public void MarkShipped_WhenNotProcessing_Throws()
        {
            var shipment = CreateValidShipment();

            Assert.Throws<InvalidOperationException>(() => shipment.MarkShipped());
        }

        [Fact]
        public void MarkInTransit_FromShipped_Succeeds()
        {
            var shipment = CreateValidShipment();
            shipment.TakeIntoProcessing();
            shipment.MarkShipped();
            shipment.MarkInTransit();

            Assert.Equal(ShipmentStatus.InTransit, shipment.Status);
        }

        [Fact]
        public void MarkOutForDelivery_FromInTransit_Succeeds()
        {
            var shipment = CreateValidShipment();
            shipment.TakeIntoProcessing();
            shipment.MarkShipped();
            shipment.MarkInTransit();
            shipment.MarkOutForDelivery();

            Assert.Equal(ShipmentStatus.OutForDelivery, shipment.Status);
        }

        [Fact]
        public void MarkDelivered_FromOutForDelivery_Succeeds()
        {
            var shipment = CreateValidShipment();
            shipment.TakeIntoProcessing();
            shipment.MarkShipped();
            shipment.MarkInTransit();
            shipment.MarkOutForDelivery();
            shipment.MarkDelivered();

            Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        }

        [Fact]
        public void MarkDelivered_WhenNotOutForDelivery_Throws()
        {
            var shipment = CreateValidShipment();

            Assert.Throws<InvalidOperationException>(() => shipment.MarkDelivered());
        }

        [Fact]
        public void MarkDelivered_FromOutForDelivery_SetsDeliveredAt()
        {
            var shipment = CreateValidShipment();
            shipment.TakeIntoProcessing();
            shipment.MarkShipped();
            shipment.MarkInTransit();
            shipment.MarkOutForDelivery();

            var before = DateTime.UtcNow;
            shipment.MarkDelivered();
            var after = DateTime.UtcNow;

            Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
            Assert.NotNull(shipment.DeliveredAt);
            Assert.InRange(shipment.DeliveredAt!.Value, before, after);
        }

        [Fact]
        public void Reconstruct_PreservesDeliveredAt()
        {
            var deliveredAt = DateTime.UtcNow.AddHours(-2);
            var shipment = Shipment.Reconstruct(
                Guid.NewGuid(), Guid.NewGuid(), "REF-123", ShipmentStatus.Delivered,
                DateTime.UtcNow.AddHours(-5), deliveredAt);

            Assert.Equal(deliveredAt, shipment.DeliveredAt);
        }

        [Fact]
        public void Reconstruct_PreservesAllFields()
        {
            var id = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddHours(-1);

            var shipment = Shipment.Reconstruct(id, orderId, "REF-123", ShipmentStatus.InTransit, createdAt);

            Assert.Equal(id, shipment.Id);
            Assert.Equal(orderId, shipment.OrderId);
            Assert.Equal("REF-123", shipment.TrackingNumber);
            Assert.Equal(ShipmentStatus.InTransit, shipment.Status);
            Assert.Equal(createdAt, shipment.CreatedAt);
        }
    }
}