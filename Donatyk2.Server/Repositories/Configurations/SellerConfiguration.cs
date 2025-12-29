using Donatyk2.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donatyk2.Server.Repositories.Configurations
{
    public class SellerConfiguration : IEntityTypeConfiguration<SellerEntity>
    {
        public void Configure(EntityTypeBuilder<SellerEntity> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(s => s.Description)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(s => s.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(s => s.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasOne(x => x.User);
        }
    }
}
