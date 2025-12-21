using Microsoft.EntityFrameworkCore;
using System;

namespace Donatyk2.Server.Data
{
    public class DonatykDbContext : DbContext
    {
        public DonatykDbContext(DbContextOptions<DonatykDbContext> options)
            : base(options) { }

        DbSet<LotEntity> Lots { get; set; }
        DbSet<SellerEntity> Sellers { get; set; }
    }
}
