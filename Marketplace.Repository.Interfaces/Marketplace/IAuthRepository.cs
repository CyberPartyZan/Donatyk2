namespace Marketplace.Repository
{
    public interface IAuthRepository
    {
        Task CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default);
        Task<Guid?> UseRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
        Task RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}