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

        public string Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(bytes);
        }
    }
}
