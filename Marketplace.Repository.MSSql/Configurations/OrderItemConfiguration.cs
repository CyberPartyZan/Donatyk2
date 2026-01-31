using Donatyk2.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donatyk2.Server.Repositories.Configurations
{
    internal class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
    {
        public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
        {
            builder.HasKey(oi => new { oi.OrderId, oi.LotId });

            builder.Property(oi => oi.NameSnapshot)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.OwnsOne(oi => oi.UnitPrice, moneyBuilder => 
            {
                moneyBuilder.Property(m => m.Amount)
                            .IsRequired();
                moneyBuilder.Property(m => m.Currency)
                            .HasConversion<string>()
                            .IsRequired()
                            .HasMaxLength(3);
            });

            builder.Property(oi => oi.Quantity)
                   .IsRequired();

            builder.HasOne(oi => oi.Order)
                   .WithMany(o => o.Items)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(oi => oi.Lot);
        }
    }
}
