using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Infra.Net.DataAccess.MongoDb.Repository
{
    public interface IMongoReadOnlyRepository<T> where T : MongoEntity
    {
        IMongoCollection<T> Items { get; }
        Task<IAsyncCursor<T>> Get(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default);
        Task<T> Get(string id, FindOptions<T> options = null, CancellationToken cancellationToken = default);
    }
}