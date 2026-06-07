using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql
{
    internal class ImageConfiguration : IEntityTypeConfiguration<ImageEntity>
    {
        public void Configure(EntityTypeBuilder<ImageEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ToTable("Images", t =>
            {
                t.HasCheckConstraint("CK_Images_UrlOrData", "[Url] IS NOT NULL OR [Data] IS NOT NULL");
            });

            builder.Property(x => x.Url)
                .HasMaxLength(2048)
                .IsRequired(false);

            builder.Property(x => x.Data)
                .IsRequired(false);

            builder.HasOne(x => x.Lot)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}