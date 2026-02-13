using Donatyk2.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Donatyk2.Server.Repositories.Configurations
{
    internal class LotConfiguration : IEntityTypeConfiguration<LotEntity>
    {
        public void Configure(EntityTypeBuilder<LotEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Stage)
                   .HasConversion<string>();

            builder.Property(x => x.Type)
                   .HasConversion<string>();

            builder.HasOne(x => x.Seller);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .ValueGeneratedOnAdd()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            builder.Property(x => x.DeclineReason)
                   .HasMaxLength(1024);

            builder.OwnsOne(x => x.Price, moneyBuilder => 
            {
                moneyBuilder.Property(m => m.Amount)
                            .IsRequired();
                moneyBuilder.Property(m => m.Currency)
                            .HasConversion<string>()
                            .IsRequired()
                            .HasMaxLength(3);
            });

            builder.OwnsOne(x => x.Compensation, moneyBuilder => 
            {
                moneyBuilder.Property(m => m.Amount)
                            .IsRequired();
                moneyBuilder.Property(m => m.Currency)
                            .HasConversion<string>()
                            .IsRequired()
                            .HasMaxLength(3);
            });

            builder.OwnsOne(x => x.TicketPrice, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                            .IsRequired();
                moneyBuilder.Property(m => m.Currency)
                            .HasConversion<string>()
                            .IsRequired()
                            .HasMaxLength(3);
            });
        }
    }
}
