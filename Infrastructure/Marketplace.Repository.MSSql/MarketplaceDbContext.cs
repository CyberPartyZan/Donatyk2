using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class MarketplaceDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
            : base(options) { }

        public DbSet<LotEntity> Lots { get; set; }
        public DbSet<SellerEntity> Sellers { get; set; }
        public DbSet<CartItemEntity> CartItems { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<BidEntity> BidHistory { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<TicketEntity> Tickets { get; set; }
        public DbSet<CharacteristicEntity> Characteristics { get; set; }
        public DbSet<ShipmentEntity> Shipments { get; set; }
        public DbSet<ShippingAddressEntity> ShippingAddresses { get; set; }
        public DbSet<DeliveryPreferencesEntity> DeliveryPreferences { get; set; }
        public DbSet<ImageEntity> Images { get; set; }
        public DbSet<CompensationEntity> Compensations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Register entity configurations
            modelBuilder.ApplyConfiguration(new LotConfiguration());
            modelBuilder.ApplyConfiguration(new SellerConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new CartItemConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new BidConfiguration());
            modelBuilder.ApplyConfiguration(new TicketConfiguration());
            modelBuilder.ApplyConfiguration(new CharacteristicConfiguration());
            modelBuilder.ApplyConfiguration(new ShipmentConfiguration());
            modelBuilder.ApplyConfiguration(new ShippingAddressConfiguration());
            modelBuilder.ApplyConfiguration(new DeliveryPreferencesConfiguration());
            modelBuilder.ApplyConfiguration(new ImageConfiguration());
            modelBuilder.ApplyConfiguration(new CompensationConfiguration());
        }
    }
}
