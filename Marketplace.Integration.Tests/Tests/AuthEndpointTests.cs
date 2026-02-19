using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Marketplace.Repository.MSSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marketplace.Integration.Tests;

public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private const string DefaultPassword = "P@ssw0rd1!";
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_ReturnsTokensAndRefreshCookie_ForValidCredentials()
    {
        var user = await CreateUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = DefaultPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(payload?.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload?.RefreshToken));
        Assert.NotNull(GetRefreshCookieValue(response));
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_ForInvalidPassword()
    {
        var user = await CreateUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsRedirect_WhenEmailNotConfirmed()
    {
        var user = await CreateUserAsync(emailConfirmed: false);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = DefaultPassword
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/email-confirmation", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Register_RedirectsToEmailConfirmation_AndPersistsUser()
    {
        var email = $"new-user-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest
        {
            Email = email,
            Password = DefaultPassword,
            ConfirmPassword = DefaultPassword
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.Location?.OriginalString.EndsWith(
            $"/email-confirmation?email={Uri.EscapeDataString(email)}",
            StringComparison.OrdinalIgnoreCase));

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var createdUser = await userManager.FindByEmailAsync(email);

        Assert.NotNull(createdUser);
        Assert.False(createdUser!.EmailConfirmed);
    }

    [Fact]
    public async Task Refresh_ReturnsTokens_WhenRefreshCookieProvided()
    {
        var user = await CreateUserAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = DefaultPassword
        });

        var refreshCookie = GetRefreshCookieValue(loginResponse);
        Assert.NotNull(refreshCookie);

        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshRequest.Headers.Add("Cookie", refreshCookie);

        var refreshResponse = await _client.SendAsync(refreshRequest);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var payload = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(payload?.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload?.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenRefreshCookieMissing()
    {
        var response = await _client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshTokens()
    {
        var user = await CreateUserAsync(emailConfirmed: true, email: "integration@test.com", userId: TestAuthHandler.UserId);

        await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = DefaultPassword
        });

        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutRequest.Headers.Authorization = new AuthenticationHeaderValue(TestAuthHandler.Scheme);

        var logoutResponse = await _client.SendAsync(logoutRequest);

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DonatykDbContext>();
        var tokens = await db.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();

        Assert.NotEmpty(tokens);
        Assert.All(tokens, token => Assert.True(token.IsRevoked));
    }

    [Fact]
    public async Task ConfirmEmail_MarksUserAsConfirmed()
    {
        var user = await CreateUserAsync(emailConfirmed: false);
        var encodedToken = await GenerateEmailConfirmationTokenAsync(user.Id);

        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", new ConfirmEmailRequest
        {
            UserId = user.Id.ToString(),
            Token = encodedToken
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var updated = await userManager.FindByIdAsync(user.Id.ToString());

        Assert.True(updated!.EmailConfirmed);
    }

    [Fact]
    public async Task ResetPassword_AllowsSubsequentLoginWithNewPassword()
    {
        var user = await CreateUserAsync();
        var encodedToken = await GeneratePasswordResetTokenAsync(user.Id);
        const string newPassword = "N3wP@ssw0rd!";

        var resetResponse = await _client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest
        {
            Email = user.Email,
            Token = encodedToken,
            Password = newPassword,
            ConfirmPassword = newPassword
        });

        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = newPassword
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ChangeEmail_Succeeds_ForAuthenticatedUser()
    {
        const string redirectUrl = "https://app.test/email-change";
        var originalEmail = "change-user@example.com";
        var user = await CreateUserAsync(email: originalEmail, userId: TestAuthHandler.UserId);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/change-email")
        {
            Content = JsonContent.Create(new ChangeEmailRequest
            {
                NewEmail = $"updated-{Guid.NewGuid():N}@example.com",
                Password = DefaultPassword,
                RedirectUrl = redirectUrl
            })
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(TestAuthHandler.Scheme);

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var reloaded = await userManager.FindByIdAsync(user.Id.ToString());

        Assert.Equal(originalEmail, reloaded!.Email);
        Assert.Equal(originalEmail, reloaded.UserName);
    }

    private async Task<ApplicationUser> CreateUserAsync(bool emailConfirmed = true, string? email = null, Guid? userId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        email ??= $"user-{Guid.NewGuid():N}@example.com";

        var user = new ApplicationUser
        {
            Id = userId ?? Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed,
            CreatedAt = DateTime.UtcNow,
            Password = DefaultPassword
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        if (emailConfirmed && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }
        else if (!emailConfirmed && user.EmailConfirmed)
        {
            user.EmailConfirmed = false;
            await userManager.UpdateAsync(user);
        }

        return user;
    }

    private async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException("User not found.");
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return EncodeToken(token);
    }

    private async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException("User not found.");
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return EncodeToken(token);
    }

    private static string EncodeToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    private static string? GetRefreshCookieValue(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return null;
        }

        var cookie = cookies.FirstOrDefault(c => c.StartsWith("refreshToken=", StringComparison.OrdinalIgnoreCase));
        return cookie?.Split(';', 2)[0];
    }
}