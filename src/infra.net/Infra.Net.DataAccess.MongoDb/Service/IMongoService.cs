using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Infra.Net.DataAccess.MongoDb.Repository;

namespace Infra.Net.DataAccess.MongoDb.Service;

public interface IMongoService<T> : IMongoReadOnlyService<T> where T : MongoEntity
{
    Task<T> Create(T entity, InsertOneOptions options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> Create(IEnumerable<T> entity, InsertManyOptions options = null, CancellationToken cancellationToken = default);
    Task<T> Update(string id, T entityIn, ReplaceOptions options = null, CancellationToken cancellationToken = default);
    Task<long> Remove(T entityIn, DeleteOptions options = null, CancellationToken cancellationToken = default);
    Task<long> Remove(IEnumerable<T> entityIn, DeleteOptions options = null, CancellationToken cancellationToken = default);
    Task<long> Remove(string id, DeleteOptions options = null, CancellationToken cancellationToken = default);
    Task<long> Remove(IEnumerable<string> id, DeleteOptions options = null, CancellationToken cancellationToken = default);
}