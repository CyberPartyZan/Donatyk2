using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Marketplace.Cache
{
    public class DistributedCacheService : IDistributedCache
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly IDatabase _database;
        private readonly TimeSpan _defaultExpiration;

        public DistributedCacheService(
            IConnectionMultiplexer connectionMultiplexer,
            IOptions<RedisSettings> redisOptions)
        {
            _database = connectionMultiplexer.GetDatabase();

            var settings = redisOptions?.Value ?? new RedisSettings();
            var cacheMinutes = Math.Max(settings.CacheExpirationMinutes, 1);
            _defaultExpiration = TimeSpan.FromMinutes(cacheMinutes);
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            var redisValue = _database.StringGet(key);

            if (redisValue.IsNullOrEmpty)
            {
                value = default!;
                return false;
            }

            value = JsonSerializer.Deserialize<T>(redisValue.ToString(), SerializerOptions)!;
            return true;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            var serialized = JsonSerializer.Serialize(value, SerializerOptions);
            _database.StringSet(key, serialized, expiration ?? _defaultExpiration);
        }
    }
}
