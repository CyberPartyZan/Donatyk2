namespace Marketplace.Cache
{
    public interface IDistributedCache
    {
        bool TryGet<T>(string key, out T value);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
    }
}
