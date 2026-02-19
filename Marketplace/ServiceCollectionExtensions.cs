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
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddScoped<ICategoriesService, CategoryService>();
            services.AddSingleton<IPaymentGateway, FakePaymentGateway>();

            return services;
        }
    }
}
