using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Net.CacheManager
{
    public interface IBinaryCacheManager
    {
        ServerPool ServerPool { get; }
        void AddServer(Uri uri);
        void RemoveServer(Uri uri);

        Task<bool> Touch(string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
          TimeSpan? idleTime = null, CancellationToken cancellationToken = default);

        Task<T> GetOrPut<T>(string id, string key, Func<Task<T>> factory,
           IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null, TimeSpan? idleTime = null, CancellationToken cancellationToken = default) where T : class;

        Task<T> Get<T>(string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
           TimeSpan? idleTime = null, CancellationToken cancellationToken = default) where T : class;

        bool Put(string id, string key, dynamic value, IEnumerable<string> compositeKey = null,
           TimeSpan? expireTime = null, TimeSpan? idleTime = null, CancellationToken cancellationToken = default);

        Task<bool> PutAsync(string id, string key, dynamic value,
           IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
           TimeSpan? idleTime = null, CancellationToken cancellationToken = default);

        bool Delete(string id, string key = null, IEnumerable<string> compositeKey = null, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(string id, string key = null, IEnumerable<string> compositeKey = null, CancellationToken cancellationToken = default);
    }
}