using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal sealed class CompensationConfiguration : IEntityTypeConfiguration<CompensationEntity>
    {
        public void Configure(EntityTypeBuilder<CompensationEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.OwnsOne(x => x.Amount, money =>
            {
                money.Property(m => m.Amount).IsRequired().HasColumnName("AmountValue");
                money.Property(m => m.Currency)
                    .HasConversion<string>()
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasColumnName("AmountCurrency");
            });

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Lot)
                .WithMany()
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApprovementDocument)
                .WithMany()
                .HasForeignKey(x => x.ApprovementDocumentId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasIndex(x => new { x.OrderId, x.LotId }).IsUnique();
        }
    }
}