using Newtonsoft.Json;

namespace Infra.Net.CacheManager;

public interface ICacheManager
{
    ServerPool ServerPool { get; }
    void AddServer(Uri uri);
    void RemoveServer(Uri uri);

    Task<HttpResponseMessage> Touch(string id, string key, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, CancellationToken cancellationToken = default);

    Task<T> GetOrPut<T>(string id, string key, Func<Task<T>> factory, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, bool? performAsync = null, CancellationToken cancellationToken = default);

    Task<string> GetOrPut(
        string id, string key, Func<Task<string>> factory, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null,
        JsonSerializerSettings jsonProps = null, bool? performAsync = null, CancellationToken cancellationToken = default);

    Task<string> Get(
        string id, string key, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, CancellationToken cancellationToken = default);

    Task<T> Get<T>(
        string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
        TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, CancellationToken cancellationToken = default) where T : class;

    void Put(string id, string key, dynamic value, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null, TimeSpan? idleTime = null,
        JsonSerializerSettings jsonProps = null, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PutAsync(
        string id, string key, dynamic value, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, bool? performAsync = null, JsonSerializerSettings jsonProps = null,
        CancellationToken cancellationToken = default);

    void Delete(string id, string key = null, IEnumerable<string> compositeKey = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DeleteAsync(string id, string key = null, IEnumerable<string> compositeKey = null, bool? performAsync = null,
        CancellationToken cancellationToken = default);
}