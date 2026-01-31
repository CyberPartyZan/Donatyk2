using Donatyk2.Server.Data;
using Donatyk2.Server.Services.Interfaces;
using System.Security.Cryptography;

namespace Donatyk2.Server.Services
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public RefreshTokenGenerator()
        {
        }

        public string Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(bytes);
        }
    }
}
