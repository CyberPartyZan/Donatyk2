using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;

namespace Donatyk2.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> CreateTokensAsync(ApplicationUser user);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshRequest request);
        Task LogoutAsync();
    }
}
