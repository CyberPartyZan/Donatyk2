using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class CharacteristicConfiguration : IEntityTypeConfiguration<CharacteristicEntity>
    {
        public void Configure(EntityTypeBuilder<CharacteristicEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Value).IsRequired().HasMaxLength(1000);

            builder.HasOne(x => x.Lot)
                .WithMany(x => x.Characteristics)
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}