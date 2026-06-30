using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class ShipmentConfiguration : IEntityTypeConfiguration<ShipmentEntity>
    {
        public void Configure(EntityTypeBuilder<ShipmentEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderId)
                   .IsRequired();

            builder.Property(x => x.TrackingNumber)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(x => x.Carrier)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.DeliveredAt)
                   .IsRequired(false);

            builder.HasOne(x => x.ShippingAddress)
                   .WithOne(a => a.Shipment)
                   .HasForeignKey<ShipmentEntity>(x => x.ShippingAddressId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                   .WithOne(o => o.Shipment)
                   .HasForeignKey<ShipmentEntity>(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}