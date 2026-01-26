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
            var sellersQuery = _db.Sellers
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sellersQuery = sellersQuery.Where(s => s.Name.Contains(search) || s.Description.Contains(search));
            }

            sellersQuery = sellersQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var entities = await sellersQuery.ToListAsync();

            return entities.Select(e => new Seller(
                e.Id,
                e.Name,
                e.Description,
                e.Email,
                e.PhoneNumber,
                e.AvatarImageUrl,
                e.UserId));
        }

        public async Task<Seller?> GetById(Guid id)
        {
            var entity = await _db.Sellers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            return entity is null ? null : new Seller(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Email,
                entity.PhoneNumber,
                entity.AvatarImageUrl,
                entity.UserId);
        }

        public Task<Seller?> GetByUserId(Guid userId)
        {
            return _db.Sellers
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .Select(entity => new Seller(
                    entity.Id,
                    entity.Name,
                    entity.Description,
                    entity.Email,
                    entity.PhoneNumber,
                    entity.AvatarImageUrl,
                    entity.UserId))
                .FirstOrDefaultAsync();
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
                AvatarImageUrl = seller.AvatarImageUrl ?? string.Empty,
                UserId = seller.UserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _db.Sellers.Add(entity);

            await _db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task Update(Seller seller)
        {
            var existing = await _db.Sellers.FirstOrDefaultAsync(e => e.Id == seller.Id);

            if (existing is null || existing.IsDeleted)
            {
                throw new KeyNotFoundException($"Seller with id '{seller.Id}' not found.");
            }

            // Update mutable fields only
            existing.Name = seller.Name;
            existing.Description = seller.Description;
            existing.Email = seller.Email;
            existing.PhoneNumber = seller.PhoneNumber;
            existing.AvatarImageUrl = seller.AvatarImageUrl ?? existing.AvatarImageUrl;
            // preserve existing.UserId and CreatedAt

            _db.Sellers.Update(existing);

            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var existing = await _db.Sellers.FirstOrDefaultAsync(e => e.Id == id);

            if (existing is null)
            {
                // nothing to do
                return;
            }

            if (!existing.IsDeleted)
            {
                existing.IsDeleted = true;
                _db.Sellers.Update(existing);
                await _db.SaveChangesAsync();
            }
        }
    }
}
