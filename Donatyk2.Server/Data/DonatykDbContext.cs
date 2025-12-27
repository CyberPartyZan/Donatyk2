using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace Donatyk2.Server.Data
{
    public class DonatykDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DonatykDbContext(DbContextOptions<DonatykDbContext> options)
            : base(options) { }

        public DbSet<LotEntity> Lots { get; set; }
        public DbSet<SellerEntity> Sellers { get; set; }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    }
}
