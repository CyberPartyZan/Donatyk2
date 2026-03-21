using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class TicketConfiguration : IEntityTypeConfiguration<TicketEntity>
    {
        public void Configure(EntityTypeBuilder<TicketEntity> builder)
        {
            builder.ToTable("Tickets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsWinning)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(x => new { x.LotId, x.UserId });

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Lot)
                .WithMany(l => l.Tickets)
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}