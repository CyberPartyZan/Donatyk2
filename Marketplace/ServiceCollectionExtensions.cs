using Donatyk2.Server.Services;
using Donatyk2.Server.Services.Interfaces;
using Donatyk2.Server.Services.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarketplaceServices(this IServiceCollection services)
        {
            services.AddScoped<ILotsService, LotsService>();
            services.AddScoped<ISellersService, SellersService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddSingleton<IPaymentGateway, FakePaymentGateway>();

            return services;
        }
    }
}
