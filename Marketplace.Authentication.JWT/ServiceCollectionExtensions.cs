using Donatyk2.Server.Services;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace Marketplace.Authentication.JWT
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

            return services;
        }
    }
}
