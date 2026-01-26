using Azure.Core;
using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Security.Claims;

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

        public AuthService(
            ClaimsPrincipal user,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DonatykDbContext db, 
            IJwtTokenGenerator jwt,
            IRefreshTokenGenerator refreshTokenGenerator)
        {
            _user = user;
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _jwt = jwt;
            _refreshTokenGenerator = refreshTokenGenerator;
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

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
                return null;

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

            // 🔥 Rotation
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

            return;
        }
    }
}
