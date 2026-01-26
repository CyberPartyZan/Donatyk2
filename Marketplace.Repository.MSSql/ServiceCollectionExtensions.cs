using Donatyk2.Server.Data;
using Donatyk2.Server.Repositories;
using Donatyk2.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Marketplace.Repository.MSSql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarketplaceRepositoryServices(this IServiceCollection services, string connectionString)
        {
            // Add services to the container.
            services.AddDbContext<DonatykDbContext>(options =>
                options.UseSqlServer(connectionString));

            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<DonatykDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            services.AddScoped<ILotsRepository, LotsRepository>();
            services.AddScoped<ISellersRepository, SellersRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrdersRepository, OrdersRepository>();

            return services;
        }
    }
}
