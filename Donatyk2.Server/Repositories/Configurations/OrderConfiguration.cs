using Donatyk2.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donatyk2.Server.Repositories.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
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

            builder.Property(x => x.ShippingRecipientName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.ShippingLine1)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(x => x.ShippingLine2)
                   .HasMaxLength(250);

            builder.Property(x => x.ShippingCity)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.ShippingState)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.ShippingPostalCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.ShippingCountry)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.ShippingPhone)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(x => x.PaymentProvider)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.PaymentReturnUrl)
                   .HasMaxLength(500);

            builder.Property(x => x.PaymentReference)
                   .HasMaxLength(200);

            builder.Property(x => x.PaymentTaxRate)
                   .HasPrecision(5, 4);

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

            builder.HasMany(x => x.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
