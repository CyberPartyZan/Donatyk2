// TODO: Namespace should be Repository?
namespace Marketplace.Authentication
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
        Task ChangeEmailAsync(ChangeEmailRequest request);
        Task ForgotPassword(string email);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task ReSendEmailConfirmationAsync(string email, string? redirectUrl);
    }
}
