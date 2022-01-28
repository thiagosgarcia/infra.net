namespace Infra.Net.CacheManager.Redis;

public interface IRedisCacheManager
{
    Task TouchAsync(string key, TimeSpan? expireTime = null);

    Task<T> GetOrPutAsync<T>(string key, Func<Task<T>> factory,
        TimeSpan? expireTime = null, bool replaceExisting = true) where T : class;

    Task<T> GetAsync<T>(string key) where T : class;
    Task PutAsync<T>(string key, T value, TimeSpan? expireTime = null, bool replaceExisting = true) where T : class;
    Task DeleteAsync(string key);
}