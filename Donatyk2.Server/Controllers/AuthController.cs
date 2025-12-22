using Azure;
using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

            _response.Cookies.Append(
                RefreshCookieName,
                tokens.RefreshToken,
                RefreshCookieOptions);

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

            var tokens = await _authService.RegisterAsync(request);
            if (tokens is null)
                return Results.BadRequest("User registration failed.");

            _response.Cookies.Append(
                RefreshCookieName,
                tokens.RefreshToken,
                RefreshCookieOptions);

            return Results.Ok(tokens);
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

            _response.Cookies.Append(
                RefreshCookieName,
                tokens.RefreshToken,
                RefreshCookieOptions);

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
