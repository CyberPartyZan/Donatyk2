using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class CartItemConfiguration : IEntityTypeConfiguration<CartItemEntity>
    {
        public void Configure(EntityTypeBuilder<CartItemEntity> builder)
        {
            builder.HasKey(c => new { c.LotId, c.UserId });

            builder.HasOne(c => c.Lot)
                   .WithMany()
                   .HasForeignKey(c => c.LotId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.User)
                   .WithMany()
                   .HasForeignKey(c => c.UserId)
                   // TODO: Check if Cascade will delete CartItems when User is deleted
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
