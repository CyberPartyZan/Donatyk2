using Donatyk2.Server.Dto;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Donatyk2.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly HttpResponse _response;
        private readonly HttpRequest _request;

        private const string RefreshCookieName = "refreshToken";
        private static CookieOptions RefreshCookieOptions =>
            new()
            {
                HttpOnly = true,
                Secure = true,           // HTTPS only
                SameSite = SameSiteMode.Strict, // or Lax
                Expires = DateTime.UtcNow.AddDays(14),
                // TODO: Check if works properly
                Path = "/api/auth/refresh"
            };

        public AuthController(IAuthService authService, HttpResponse response, HttpRequest request)
        {
            _authService = authService;
            _response = response;
            _request = request;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest("Email and Password are required.");

            var tokens = await _authService.LoginAsync(request);
            if (tokens is null)
                return Results.Unauthorized();

            if (tokens.RequiresEmailConfirmation == true)
            {
                var redirectUrl = "/email-confirmation";
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    redirectUrl = $"/email-confirmation?email={Uri.EscapeDataString(request.Email)}";
                }

                return Results.Redirect(redirectUrl, permanent: false);
            }

            _response.Cookies.Append(RefreshCookieName, tokens.RefreshToken, RefreshCookieOptions);
            return Results.Ok(tokens);
        }

        [AllowAnonymous]
        [HttpPost("login/recovery-code")]
        public async Task<IResult> LoginWithRecoveryCode([FromBody] LoginWithRecoveryCodeRequest request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.RecoveryCode))
            {
                return Results.BadRequest("Email and RecoveryCode are required.");
            }

            var tokens = await _authService.LoginWithRecoveryCode(request);
            if (tokens is null)
                return Results.Unauthorized();

            _response.Cookies.Append(RefreshCookieName, tokens.RefreshToken, RefreshCookieOptions);
            return Results.Ok(tokens);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IResult> Register([FromBody] RegisterUserRequest request)
        {
            if (request.Password != request.ConfirmPassword)
                return Results.BadRequest("Passwords do not match.");

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest("Email and Password are required.");

            var registrationResult = await _authService.RegisterAsync(request);
            if (registrationResult is null)
                return Results.BadRequest("User registration failed.");

            var redirectUrl = "/email-confirmation";
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                redirectUrl = $"/email-confirmation?email={Uri.EscapeDataString(request.Email)}";
            }

            return Results.Redirect(redirectUrl, permanent: false);
        }

        [AllowAnonymous]
        [HttpPost("confirm-email")]
        public async Task<IResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                return Results.BadRequest("UserId and Token are required.");
            }

            await _authService.ConfirmEmailAsync(request.UserId, request.Token);
            return Results.Ok();
        }

        [AllowAnonymous]
        [HttpPost("confirm-email-change")]
        public async Task<IResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.NewEmail) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                return Results.BadRequest("UserId, NewEmail and Token are required.");
            }

            await _authService.ConfirmEmailChangeAsync(request.UserId, request.NewEmail, request.Token);
            return Results.Ok();
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IResult> ForgotPassword([FromBody] EmailRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("Email is required.");

            await _authService.ForgotPassword(request.Email);
            return Results.Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request is null)
                return Results.BadRequest("Reset request is required.");

            await _authService.ResetPasswordAsync(request);
            return Results.Ok();
        }

        [AllowAnonymous]
        [HttpPost("resend-confirmation")]
        public async Task<IResult> ResendConfirmation([FromBody] ResendEmailConfirmationRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("Email is required.");

            await _authService.ReSendEmailConfirmationAsync(request.Email, request.RedirectUrl);
            return Results.Ok();
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IResult> Refresh()
        {
            if (!_request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Results.BadRequest("Refresh token is required.");

            var tokens = await _authService.RefreshTokenAsync(refreshToken);
            if (tokens is null)
                return Results.Unauthorized();

            _response.Cookies.Append(RefreshCookieName, tokens.RefreshToken, RefreshCookieOptions);
            return Results.Ok(tokens);
        }

        [HttpPost("logout")]
        public async Task<IResult> Logout()
        {
            await _authService.LogoutAsync();

            _response.Cookies.Delete(RefreshCookieName, RefreshCookieOptions);
            return Results.Ok();
        }
    }
}