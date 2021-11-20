using System.Data.HashFunction;
using System.Data.HashFunction.CityHash;
using System.Net;
using System.Text;

namespace Infra.Net.CacheManager;

public abstract class AbstractBinaryCacheManager : IBinaryCacheManager
{
    private readonly IHashFunction _cityHash = CityHashFactory.Instance.Create();
    private readonly string _noKeyDefinedString = "no-key-defined";

    public string Urlfy(string fragment) =>
        WebUtility.UrlEncode(fragment);

    public string Hash(string fragment) =>
        Urlfy(_cityHash.ComputeHash(Encoding.UTF8.GetBytes(fragment), 256).AsHexString());

    public string GenerateDeleteKey(string key, IEnumerable<string> compositeKey)
    {
        var deleteKey = GenerateKey(key, compositeKey);
        deleteKey = deleteKey == _noKeyDefinedString ? string.Empty : $"/{deleteKey}";
        return deleteKey;
    }

    public string GenerateKey(string key, IEnumerable<string> compositeKey)
    {
        var fullKey = key ?? string.Empty;
        if (compositeKey != null && compositeKey.Any())
            fullKey += "_" + string.Join("_", compositeKey.Select(x => x));

        if (string.IsNullOrEmpty(fullKey))
            return _noKeyDefinedString;

        return Hash(fullKey);
    }

    public void AddServer(Uri uri)
        => ServerPool.Add(uri);

    public void RemoveServer(Uri uri)
        => ServerPool.Remove(uri);

    public ServerPool ServerPool { get; } = new();

    public abstract Task<bool> Touch(string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
        TimeSpan? idleTime = null, CancellationToken cancellationToken = default);

    public abstract Task<T> GetOrPut<T>(string id, string key, Func<Task<T>> factory,
        IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null, TimeSpan? idleTime = null, CancellationToken cancellationToken = default) where T : class;

    public abstract Task<T> Get<T>(string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
        TimeSpan? idleTime = null,CancellationToken cancellationToken = default) where T : class;

    public abstract bool Put(string id, string key, dynamic value, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, CancellationToken cancellationToken = default);

    public abstract Task<bool> PutAsync(string id, string key, dynamic value,
        IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
        TimeSpan? idleTime = null, CancellationToken cancellationToken = default);

    public abstract bool Delete(string id, string key = null, IEnumerable<string> compositeKey = null, CancellationToken cancellationToken = default);
        
    public abstract Task<bool> DeleteAsync(string id, string key = null, IEnumerable<string> compositeKey = null, CancellationToken cancellationToken = default);
}