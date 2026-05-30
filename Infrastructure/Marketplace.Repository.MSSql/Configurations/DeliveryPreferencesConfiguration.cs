using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class DeliveryPreferencesConfiguration : IEntityTypeConfiguration<DeliveryPreferencesEntity>
    {
        public void Configure(EntityTypeBuilder<DeliveryPreferencesEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId).IsRequired();

            builder.Property(x => x.Carrier)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(50);

            builder.HasOne(x => x.ShippingAddress)
                   .WithOne(x => x.DeliveryPreferences)
                   .HasForeignKey<DeliveryPreferencesEntity>(x => x.ShippingAddressId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId);
        }
    }
}