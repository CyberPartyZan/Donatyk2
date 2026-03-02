using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Marketplace.Cache
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RedisSettings>()
                .Bind(configuration.GetSection(RedisSettings.SectionName))
                .Validate(settings => settings.CacheExpirationMinutes > 0, "Redis cache expiration must be greater than zero.")
                .ValidateOnStart();

            var connectionString = configuration.GetConnectionString("Redis");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            return services.AddCacheServices(connectionString);
        }

        public static IServiceCollection AddCacheServices(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
            services.AddScoped<IDistributedCache, DistributedCacheService>();

            return services;
        }
    }
}
