using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Infra.Net.DataAccess.MongoDb.Repository
{
    public interface IMongoRepository<T> : IMongoReadOnlyRepository<T> where T : MongoEntity
    {
        Task<T> Create(T entity, InsertOneOptions options = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> Create(IEnumerable<T> list, InsertManyOptions options = null, CancellationToken cancellationToken = default);
        Task<T> Update(string id, T entityIn, ReplaceOptions options = null, CancellationToken cancellationToken = default);
        Task<long> Remove(T entity, DeleteOptions options = null, CancellationToken cancellationToken = default);
        Task<long> Remove(IEnumerable<T> list, DeleteOptions options = null, CancellationToken cancellationToken = default);
        Task<long> Remove(string id, DeleteOptions options = null, CancellationToken cancellationToken = default);
        Task<long> Remove(IEnumerable<string> list, DeleteOptions options = null, CancellationToken cancellationToken = default);
    }
}