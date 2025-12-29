using Donatyk2.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donatyk2.Server.Repositories.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Token)
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(rt => rt.ExpiresAt)
                   .IsRequired();

            builder.HasOne(rt => rt.User)
                   .WithMany()
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
