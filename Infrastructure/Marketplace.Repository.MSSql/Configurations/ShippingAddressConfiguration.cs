using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddressEntity>
    {
        public void Configure(EntityTypeBuilder<ShippingAddressEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RecipientName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Line1)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(x => x.Line2)
                   .HasMaxLength(250);

            builder.Property(x => x.City)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.State)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.PostalCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.Country)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Phone)
                   .IsRequired()
                   .HasMaxLength(30);
        }
    }
}