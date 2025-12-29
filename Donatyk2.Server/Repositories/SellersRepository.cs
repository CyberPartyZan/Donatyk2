using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Donatyk2.Server.Data;
using Donatyk2.Server.Models;
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
        public async Task<IEnumerable<Seller>> GetAll(string? search, int page, int pageSize)
        {
            var sellersQuery = _db.Sellers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sellersQuery = sellersQuery.Where(s => s.Name.Contains(search) || s.Description.Contains(search));
            }

            sellersQuery = sellersQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var entities = await sellersQuery.ToListAsync();

            return entities.Select(e => new Seller(e));
        }

        public async Task<Seller?> GetById(Guid id)
        {
            var entity = await _db.Sellers.FindAsync(id);
            return entity is null ? null : new Seller(entity);
        }

        public async Task<Guid> Create(Seller seller)
        {
            var existingSeller = await _db.Sellers.FirstOrDefaultAsync(s => s.UserId == seller.UserId);
            // TODO: Move to separate method and validate in service layer?
            if (existingSeller is not null)
            {
                throw new InvalidOperationException("Seller for this user already exists.");
            }

            var entity = new SellerEntity
            {
                Id = seller.Id == Guid.Empty ? Guid.NewGuid() : seller.Id,
                Name = seller.Name,
                Description = seller.Description,
                Email = seller.Email,
                PhoneNumber = seller.PhoneNumber,
                AvatarImageUrl = seller.AvatarImageUrl,
                UserId = seller.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Sellers.Add(entity);

            await _db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task Update(Seller seller)
        {
            var entity = new SellerEntity
            {
                Id = seller.Id,
                Name = seller.Name,
                Description = seller.Description,
                Email = seller.Email,
                PhoneNumber = seller.PhoneNumber,
                AvatarImageUrl = seller.AvatarImageUrl,
                UserId = seller.UserId,
                // Do not overwrite CreatedAt here; EF will track the existing entity if attached.
            };

            _db.Sellers.Update(entity);

            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            _db.Sellers.Remove(new SellerEntity { Id = id });

            await _db.SaveChangesAsync();
        }
    }
}
