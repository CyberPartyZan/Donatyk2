using Donatyk2.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donatyk2.Server.Repositories.Configurations
{
    internal class BidConfiguration : IEntityTypeConfiguration<BidEntity>
    {
        public void Configure(EntityTypeBuilder<BidEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PlacedAt)
                   .IsRequired();

            builder.HasOne(x => x.Auction)
                   .WithMany()
                   .HasForeignKey(x => x.AuctionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Bidder)
                   .WithMany()
                   .HasForeignKey(x => x.BidderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(x => x.Amount, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                            .HasColumnName("AmountValue")
                            .IsRequired();

                moneyBuilder.Property(m => m.Currency)
                            .HasColumnName("AmountCurrency")
                            .HasConversion<string>()
                            .IsRequired()
                            .HasMaxLength(3);
            });
        }
    }
}