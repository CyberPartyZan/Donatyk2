using Donatyk2.Server.Data;
using Donatyk2.Server.Repositories.Interfaces;
using Marketplace.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace Donatyk2.Server.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DonatykDbContext _db;

        public UsersRepository(DonatykDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> GetAll(string? search, int page, int pageSize)
        {
            var usersQuery = _db.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Email!.Contains(search) ||
                    u.UserName!.Contains(search));
            }

            var entities = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return entities.Select(ToModel);
        }

        public async Task<User?> GetById(Guid id)
        {
            var entity = await _db.Users.FindAsync(id);
            return entity is null ? null : ToModel(entity);
        }

        public async Task<User?> GetByEmail(string email)
        {
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            return entity is null ? null : ToModel(entity);
        }

        public async Task Update(User user)
        {
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (entity is null)
            {
                throw new KeyNotFoundException($"User with id '{user.Id}' not found.");
            }

            ApplyDomainValues(user, entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            _db.Users.Remove(new ApplicationUser { Id = id });
            await _db.SaveChangesAsync();
        }

        private static User ToModel(ApplicationUser entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Email))
            {
                throw new InvalidOperationException("User email cannot be null or whitespace.");
            }

            return new User(
                entity.Id,
                entity.Email,
                entity.EmailConfirmed,
                entity.LockoutEnabled,
                entity.LockoutEnd);
        }

        private static void ApplyDomainValues(User source, ApplicationUser target)
        {
            target.Email = source.Email;
            target.UserName = source.Email;
            target.EmailConfirmed = source.EmailConfirmed;
            target.LockoutEnabled = source.LockoutEnabled;
            target.LockoutEnd = source.LockoutEnd;
        }
    }
}