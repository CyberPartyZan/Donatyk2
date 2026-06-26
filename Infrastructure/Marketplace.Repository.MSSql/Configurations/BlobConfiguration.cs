using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Repository.MSSql.Configurations
{
    internal class BlobConfiguration : IEntityTypeConfiguration<BlobEntity>
    {
        public void Configure(EntityTypeBuilder<BlobEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath).IsRequired();
            builder.Property(x => x.Key).IsRequired();
        }
    }
}
