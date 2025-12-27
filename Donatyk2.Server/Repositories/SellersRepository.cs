using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Donatyk2.Server.Data;
using Donatyk2.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Donatyk2.Server.Repositories
{
    public class SellersRepository : ISellersRepository
    {
        private readonly DonatykDbContext _db;

        public SellersRepository(DonatykDbContext db) 
        {
            _db = db;
        }

        // TODO: Implement keyset pagination
        public async Task<IEnumerable<SellerEntity>> GetAll(string search, int page, int pageSize)
        {
            var sellersQuery = _db.Sellers.AsQueryable();

            if (search is not null)
            {
                sellersQuery = sellersQuery.Where(s => s.Name.Contains(search) || s.Description.Contains(search));
            }

            sellersQuery = sellersQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return await _db.Sellers.ToListAsync();
        }

        public async Task<SellerEntity?> GetById(Guid id)
        {
            var seller = await _db.Sellers.FindAsync(id);

            return seller;
        }

        public async Task<Guid> Create(SellerEntity seller)
        {
            var existingSeller = await _db.Sellers.FirstOrDefaultAsync(s => s.UserId == seller.UserId);

            if (existingSeller is not null)
            {
                throw new InvalidOperationException("Seller for this user already exists.");
            }

            _db.Sellers.Add(seller);

            await _db.SaveChangesAsync();

            return seller.Id;
        }

        public async Task Update(SellerEntity seller)
        {
            _db.Sellers.Update(seller);

            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            _db.Sellers.Remove(new SellerEntity { Id = id });

            await _db.SaveChangesAsync();
        }
    }
}
