using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Repositories.Interfaces;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;

namespace Donatyk2.Server.Services
{
    // TODO: Move whole Identity to separate project? Or at least to Marketplace.Repository.MSSql?
    public class AuthService : IAuthService
    {
        private readonly ClaimsPrincipal _user;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly INotificationService _notificationService;
        private readonly IAuthRepository _authRepository;

        public AuthService(
            ClaimsPrincipal user,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenGenerator jwt,
            IRefreshTokenGenerator refreshTokenGenerator,
            INotificationService notificationService,
            IAuthRepository authRepository)
        {
            _user = user;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt;
            _refreshTokenGenerator = refreshTokenGenerator;
            _notificationService = notificationService;
            _authRepository = authRepository;
        }

        private async Task<AuthResponse> CreateTokensAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User identifier is required.", nameof(userId));
            }

            var accessToken = await _jwt.GenerateAsync(userId);

            var refreshTokenValue = _refreshTokenGenerator.Generate();
            await _authRepository.CreateRefreshTokenAsync(
                userId,
                refreshTokenValue,
                DateTime.UtcNow.AddDays(14));

            return new AuthResponse(accessToken, refreshTokenValue);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                throw new MissingFieldException("Email and Password are required.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return null;
            }

            if (!user.EmailConfirmed)
            {
                return new AuthResponse(
                    AccessToken: string.Empty,
                    RefreshToken: string.Empty,
                    RequiresEmailConfirmation: true);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return new AuthResponse(
                    AccessToken: string.Empty,
                    RefreshToken: string.Empty,
                    IsLockedOut: true);
            }

            if (!result.Succeeded)
            {
                return null;
            }

            return await CreateTokensAsync(user.Id);
        }

        public async Task<AuthResponse?> LoginWithRecoveryCode(LoginWithRecoveryCodeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.RecoveryCode))
            {
                throw new MissingFieldException("Email and RecoveryCode are required.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return null;
            }

            var normalizedCode = request.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(normalizedCode);
            if (result.IsLockedOut)
            {
                return new AuthResponse(
                    AccessToken: string.Empty,
                    RefreshToken: string.Empty,
                    IsLockedOut: true);
            }

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Invalid recovery code.");
            }

            return await CreateTokensAsync(user.Id);
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                throw new MissingFieldException("Email, Password and ConfirmPassword are required.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                // TODO: HINT: WARNING: Do NOT store passwords in plain text. This is just for demonstration purposes.
                Password = request.Password
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return null;
            }

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

            await _notificationService.NotifyEmailConfirmationAsync(
                user.Id,
                user.Email ?? request.Email!,
                encodedToken);

            return await CreateTokensAsync(user.Id);
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            var userId = await _authRepository.UseRefreshTokenAsync(refreshToken);
            if (userId is null)
            {
                return null;
            }

            return await CreateTokensAsync(userId.Value);
        }

        public async Task LogoutAsync()
        {
            var userId = Guid.Parse(
                _user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            await _authRepository.RevokeRefreshTokensAsync(userId);
        }

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User identifier is required.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Confirmation token is required.", nameof(token));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                throw new InvalidOperationException($"User '{userId}' was not found.");
            }

            if (user.EmailConfirmed)
            {
                return;
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(
                    Environment.NewLine,
                    result.Errors.Select(error => $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException($"Email confirmation failed: {errorMessage}");
            }
        }

        public async Task ConfirmEmailChangeAsync(string userId, string newEmail, string token)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User identifier is required.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(newEmail))
            {
                throw new ArgumentException("New email is required.", nameof(newEmail));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Confirmation token is required.", nameof(token));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                throw new InvalidOperationException($"User '{userId}' was not found.");
            }

            if (string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase) && user.EmailConfirmed)
            {
                return;
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var changeEmailResult = await _userManager.ChangeEmailAsync(user, newEmail, code);
            if (!changeEmailResult.Succeeded)
            {
                var errorMessage = string.Join(
                    Environment.NewLine,
                    changeEmailResult.Errors.Select(error => $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException($"Email change failed: {errorMessage}");
            }

            if (!string.Equals(user.UserName, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, newEmail);
                if (!setUserNameResult.Succeeded)
                {
                    var errorMessage = string.Join(
                        Environment.NewLine,
                        setUserNameResult.Errors.Select(error => $"{error.Code}: {error.Description}"));

                    throw new InvalidOperationException($"Email change succeeded but updating username failed: {errorMessage}");
                }
            }
        }

        public async Task ChangeEmailAsync(ChangeEmailRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.NewEmail))
            {
                throw new ArgumentException("New email is required.", nameof(request.NewEmail));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Password is required.", nameof(request.Password));
            }

            if (string.IsNullOrWhiteSpace(request.RedirectUrl))
            {
                throw new ArgumentException("RedirectUrl is required.", nameof(request.RedirectUrl));
            }

            var userIdClaim = _user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new InvalidOperationException("Authenticated user context is required.");
            }

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user is null)
            {
                throw new InvalidOperationException($"User '{userIdClaim}' was not found.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                throw new InvalidOperationException("The provided password is invalid.");
            }

            if (string.Equals(user.Email, request.NewEmail, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var existing = await _userManager.FindByEmailAsync(request.NewEmail);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new InvalidOperationException("The requested email is already in use.");
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = BuildEmailConfirmationUrl(request.RedirectUrl, user.Id, encodedToken);

            await _notificationService.NotifyEmailConfirmationAsync(
                user.Id,
                request.NewEmail,
                encodedToken,
                confirmationUrl);
        }

        public async Task ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.", nameof(email));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return;
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            await _notificationService.NotifyPasswordResetAsync(user.Id, user.Email ?? email, encodedToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("Email is required.", nameof(request.Email));
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                throw new ArgumentException("Token is required.", nameof(request.Token));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Password is required.", nameof(request.Password));
            }

            if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                throw new ArgumentException("ConfirmPassword is required.", nameof(request.ConfirmPassword));
            }

            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Password and ConfirmPassword must match.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return;
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Reset token is invalid.", ex);
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, decodedToken, request.Password);
            if (!resetResult.Succeeded)
            {
                var errorMessage = string.Join(
                    Environment.NewLine,
                    resetResult.Errors.Select(error => $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException($"Password reset failed: {errorMessage}");
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await _authRepository.RevokeRefreshTokensAsync(user.Id);
        }

        public async Task ReSendEmailConfirmationAsync(string email, string? redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.", nameof(email));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return;
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = BuildEmailConfirmationUrl(redirectUrl, user.Id, encodedToken);

            await _notificationService.NotifyEmailConfirmationAsync(
                user.Id,
                user.Email ?? email,
                encodedToken,
                confirmationUrl);
        }

        private static string BuildEmailConfirmationUrl(string redirectUrl, Guid userId, string encodedToken)
        {
            var url = QueryHelpers.AddQueryString(redirectUrl, "userId", userId.ToString());
            url = QueryHelpers.AddQueryString(url, "token", encodedToken);
            return url;
        }
    }
}
