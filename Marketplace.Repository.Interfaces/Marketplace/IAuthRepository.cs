using System;
using System.Threading;
using System.Threading.Tasks;

namespace Donatyk2.Server.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default);
        Task<Guid?> UseRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
        Task RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}