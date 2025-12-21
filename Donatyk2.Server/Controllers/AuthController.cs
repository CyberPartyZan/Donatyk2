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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

            return Results.Ok(tokens);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Results.BadRequest("Refresh token is required.");

            var tokens = await _authService.RefreshTokenAsync(request);
            if (tokens is null)
                return Results.Unauthorized();

            return Results.Ok(tokens);
        }

        [HttpPost("logout")]
        public async Task<IResult> Logout()
        {
            await _authService.LogoutAsync();

            return Results.Ok();
        }
    }
}
