using Donatyk2.Server.Dto;

namespace Donatyk2.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> LoginWithRecoveryCode(LoginWithRecoveryCodeRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterUserRequest request);
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync();

        Task ConfirmEmailAsync(string userId, string token);
        Task ConfirmEmailChangeAsync(string userId, string newEmail, string token);
        Task ForgotPassword(string email);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task ReSendEmailConfirmationAsync(string email, string? redirectUrl);
    }
}
