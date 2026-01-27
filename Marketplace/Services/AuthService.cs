using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;

namespace Donatyk2.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly ClaimsPrincipal _user;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly DonatykDbContext _db;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly INotificationService _notificationService;

        public AuthService(
            ClaimsPrincipal user,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DonatykDbContext db,
            IJwtTokenGenerator jwt,
            IRefreshTokenGenerator refreshTokenGenerator,
            INotificationService notificationService)
        {
            _user = user;
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _jwt = jwt;
            _refreshTokenGenerator = refreshTokenGenerator;
            _notificationService = notificationService;
        }

        private async Task<AuthResponse> CreateTokensAsync(ApplicationUser user)
        {
            var accessToken = await _jwt.GenerateAsync(user);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = _refreshTokenGenerator.Generate(),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return new AuthResponse(accessToken, refreshToken.Token);
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
                return null;

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
                return null;

            return await CreateTokensAsync(user);
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

            return await CreateTokensAsync(user);
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
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return null;

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

            await _notificationService.NotifyEmailConfirmationAsync(
                user.Id,
                user.Email ?? request.Email!,
                encodedToken);

            return await CreateTokensAsync(user);
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            var dbRefreshToken = await _db.RefreshTokens
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r =>
                    r.Token == refreshToken &&
                    !r.IsRevoked &&
                    r.ExpiresAt > DateTime.UtcNow);

            if (dbRefreshToken == null)
                return null;

            dbRefreshToken.IsRevoked = true;

            var tokens = await CreateTokensAsync(dbRefreshToken.User);

            await _db.SaveChangesAsync();

            return tokens;
        }

        public async Task LogoutAsync()
        {
            var userId = Guid.Parse(
                _user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            var tokens = await _db.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();

            tokens.ForEach(t => t.IsRevoked = true);
            await _db.SaveChangesAsync();
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

            var refreshTokens = await _db.RefreshTokens
                .Where(r => r.UserId == user.Id && !r.IsRevoked)
                .ToListAsync();

            if (refreshTokens.Count > 0)
            {
                refreshTokens.ForEach(t => t.IsRevoked = true);
                await _db.SaveChangesAsync();
            }
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
