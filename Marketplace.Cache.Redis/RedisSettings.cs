namespace Marketplace.Cache
{
    public class RedisSettings
    {
        public const string SectionName = "Redis";

        public int CacheExpirationMinutes { get; set; } = 10;
    }
}