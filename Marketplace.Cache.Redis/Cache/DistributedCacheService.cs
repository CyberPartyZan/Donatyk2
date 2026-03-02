using System.Text.Json;
using StackExchange.Redis;

namespace Marketplace.Cache
{
    public class DistributedCacheService : IDistributedCache
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly IDatabase _database;

        public DistributedCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
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
            _database.StringSet(key, serialized, expiration);
        }
    }
}
