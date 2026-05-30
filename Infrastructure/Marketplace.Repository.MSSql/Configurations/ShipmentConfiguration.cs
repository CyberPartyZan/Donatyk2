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

            builder.Property(x => x.ShippingReference)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.HasOne(x => x.Order)
                   .WithOne(o => o.Shipment)
                   .HasForeignKey<ShipmentEntity>(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}