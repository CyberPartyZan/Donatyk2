using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
    {
        public void Configure(EntityTypeBuilder<OrderEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(x => x.CustomerId)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.PaymentProvider)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.PaymentReturnUrl)
                   .HasMaxLength(500);

            builder.Property(x => x.PaymentReference)
                   .HasMaxLength(200);

            builder.Property(x => x.PaymentTaxRate)
                   .HasPrecision(5, 4);

            builder.Property(x => x.ShipmentId);

            builder.Property(x => x.DeliveryCarrier)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.OwnsOne(x => x.Total, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                            .IsRequired()
                            .HasColumnName("TotalAmount");

                moneyBuilder.Property(m => m.Currency)
                            .IsRequired()
                            .HasConversion<string>()
                            .HasMaxLength(3)
                            .HasColumnName("TotalCurrency");
            });

            builder.HasOne(x => x.ShippingAddress)
                   .WithOne(a => a.Order)
                   .HasForeignKey<OrderEntity>(x => x.ShippingAddressId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
