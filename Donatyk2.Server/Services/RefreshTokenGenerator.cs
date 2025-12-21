using Donatyk2.Server.Data;
using Donatyk2.Server.Services.Interfaces;
using System.Security.Cryptography;

namespace Donatyk2.Server.Services
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public readonly DonatykDbContext _db;
        public RefreshTokenGenerator(DonatykDbContext db)
        {
            _db = db;
        }

        public string Generate(Guid userId)
        {
            var bytes = RandomNumberGenerator.GetBytes(64);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Convert.ToBase64String(bytes),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            };

            return refreshToken.Token;
        }
    }
}
