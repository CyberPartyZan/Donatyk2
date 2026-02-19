using System.Security.Cryptography;

namespace Marketplace.Authentication.JWT
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
