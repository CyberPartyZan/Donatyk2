using Donatyk2.Server.Dto;

namespace Donatyk2.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterUserRequest request);
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync();
    }
}
