using Marketplace.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Repository.MSSql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarketplaceRepositoryServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<MarketplaceDbContext>(options =>
                options.UseSqlServer(connectionString));

            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<MarketplaceDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            services.AddScoped<ILotsRepository, LotsRepository>();
            services.AddScoped<ISellersRepository, SellersRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrdersRepository, OrdersRepository>();
            services.AddScoped<ICategoriesRepository, CategoriesRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();

            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
