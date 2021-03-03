using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Infra.Net.DataAccess.MongoDb.Repository;

namespace Infra.Net.DataAccess.MongoDb.Service
{
    public interface IMongoReadOnlyService<T> where T : MongoEntity
    {
        IMongoCollection<T> Items { get; }

        Task<bool> Exists(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> Get(int page = 0, int perPage = 10, Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default);
        Task<T> Get(string id, FindOptions<T> options = null, CancellationToken cancellationToken = default);

        Task<T> GetFirst(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default);
        Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default);

        Task<long> Count(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default);
    }
}