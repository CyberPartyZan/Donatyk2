using Marketplace.Repository.MSSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Marketplace.Server.Identity
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAdminUserAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<AdminUserOptions>>().Value;

            if (!options.IsValid())
            {
                return;
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await roleManager.RoleExistsAsync(options.Role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(options.Role));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{options.Role}': {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            var user = await userManager.FindByEmailAsync(options.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = options.Email,
                    Email = options.Email,
                    EmailConfirmed = options.EmailConfirmed,
                    CreatedAt = DateTime.UtcNow,
                    Password = options.Password
                };

                var createResult = await userManager.CreateAsync(user, options.Password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create admin user '{options.Email}': {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, options.Role))
            {
                var addRoleResult = await userManager.AddToRoleAsync(user, options.Role);
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to assign role '{options.Role}' to '{options.Email}': {string.Join("; ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}