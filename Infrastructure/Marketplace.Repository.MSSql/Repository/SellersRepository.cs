using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class SellersRepository : ISellersRepository
    {
        private readonly MarketplaceDbContext _db;

        public SellersRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Seller>> GetAll(string? search, int page, int pageSize)
        {
            var sellersQuery = _db.Sellers
                .AsNoTracking()
                .Include(s => s.Avatar)
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sellersQuery = sellersQuery.Where(s => s.Name.Contains(search) || s.Description.Contains(search));
            }

            var entities = await sellersQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return entities.Select(MapToDomain);
        }

        public async Task<Seller?> GetById(Guid id)
        {
            var entity = await _db.Sellers
                .AsNoTracking()
                .Include(s => s.Avatar)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task<Seller?> GetByUserId(Guid userId)
        {
            var entity = await _db.Sellers
                .AsNoTracking()
                .Include(s => s.Avatar)
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task<Guid> Create(Seller seller)
        {
            var existingSeller = await _db.Sellers.FirstOrDefaultAsync(s => s.UserId == seller.UserId);
            if (existingSeller is not null)
                throw new InvalidOperationException("Seller for this user already exists.");

            BlobEntity? avatar = null;
            if (seller.Avatar is not null)
            {
                avatar = new BlobEntity
                {
                    Id = seller.Avatar.Id,
                    FilePath = seller.Avatar.FilePath,
                    Key = seller.Avatar.Key,
                    FileName = seller.Avatar.FileName
                };
                _db.Blobs.Add(avatar);
            }

            var entity = new SellerEntity
            {
                Id = seller.Id == Guid.Empty ? Guid.NewGuid() : seller.Id,
                Name = seller.Name,
                Description = seller.Description,
                Email = seller.Email,
                PhoneNumber = seller.PhoneNumber,
                AvatarId = avatar?.Id,
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
            var existing = await _db.Sellers
                .Include(s => s.Avatar)
                .FirstOrDefaultAsync(e => e.Id == seller.Id);

            if (existing is null || existing.IsDeleted)
                throw new KeyNotFoundException($"Seller with id '{seller.Id}' not found.");

            existing.Name = seller.Name;
            existing.Description = seller.Description;
            existing.Email = seller.Email;
            existing.PhoneNumber = seller.PhoneNumber;

            if (seller.Avatar is not null)
            {
                var avatarEntity = await _db.Blobs.FirstOrDefaultAsync(b => b.Id == seller.Avatar.Id);
                if (avatarEntity is null)
                {
                    avatarEntity = new BlobEntity
                    {
                        Id = seller.Avatar.Id,
                        FilePath = seller.Avatar.FilePath,
                        Key = seller.Avatar.Key,
                        FileName = seller.Avatar.FileName
                    };
                    _db.Blobs.Add(avatarEntity);
                }
                else
                {
                    avatarEntity.FilePath = seller.Avatar.FilePath;
                    avatarEntity.Key = seller.Avatar.Key;
                    avatarEntity.FileName = seller.Avatar.FileName;
                }

                existing.AvatarId = avatarEntity.Id;
            }

            _db.Sellers.Update(existing);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var existing = await _db.Sellers.FirstOrDefaultAsync(e => e.Id == id);
            if (existing is null) return;

            if (!existing.IsDeleted)
            {
                existing.IsDeleted = true;
                _db.Sellers.Update(existing);
                await _db.SaveChangesAsync();
            }
        }

        private static Seller MapToDomain(SellerEntity e) =>
            new(
                e.Id,
                e.Name,
                e.Description,
                e.Email,
                e.PhoneNumber,
                e.Avatar is null
                    ? null
                    : new Blob(e.Avatar.Id, e.Avatar.FilePath, e.Avatar.Key, e.Avatar.FileName),
                e.UserId);
    }
}
