using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Donatyk2.Server.Data;
using Donatyk2.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Donatyk2.Server.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DonatykDbContext _db;

        public AuthRepository(DonatykDbContext db)
        {
            _db = db;
        }

        public async Task CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                UserId = userId,
                ExpiresAt = expiresAt
            };

            await _db.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Guid?> UseRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var refreshToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(
                    r => r.Token == token &&
                         !r.IsRevoked &&
                         r.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

            if (refreshToken is null)
            {
                return null;
            }

            refreshToken.IsRevoked = true;
            await _db.SaveChangesAsync(cancellationToken);

            return refreshToken.UserId;
        }

        public async Task RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _db.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync(cancellationToken);

            if (tokens.Count == 0)
            {
                return;
            }

            foreach (var refreshToken in tokens)
            {
                refreshToken.IsRevoked = true;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}