using Donatyk2.Server.Data;
using Donatyk2.Server.Repositories.Interfaces;
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

        public async Task<IEnumerable<ApplicationUser>> GetAll(string? search, int page, int pageSize)
        {
            var usersQuery = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Email!.Contains(search) ||
                    u.UserName!.Contains(search));
            }

            usersQuery = usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return await usersQuery.ToListAsync();
        }

        public async Task<ApplicationUser?> GetById(Guid id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task<ApplicationUser?> GetByEmail(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Guid> Create(ApplicationUser user)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existing is not null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
            user.CreatedAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user.Id;
        }

        public async Task Update(ApplicationUser user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            _db.Users.Remove(new ApplicationUser { Id = id });
            await _db.SaveChangesAsync();
        }
    }
}