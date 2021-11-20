using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Infra.Net.DataAccess.MongoDb.Repository;

namespace Infra.Net.DataAccess.MongoDb.Service;

public class MongoService<T> : MongoReadOnlyService<T>, IMongoService<T> where T : MongoEntity
{
    public MongoService(IMongoRepository<T> repository) 
        : base(repository)
    {
    }

    public virtual Task<T> Create(T entity, InsertOneOptions options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Create(entity, options, cancellationToken);
    }

    public virtual Task<IEnumerable<T>> Create(IEnumerable<T> entity, InsertManyOptions options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Create(entity, options, cancellationToken);
    }

    public virtual Task<T> Update(string id, T entityIn, ReplaceOptions options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Update(id, entityIn, options, cancellationToken);
    }

    public virtual Task<long> Remove(T entityIn, DeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Remove(entityIn, options, cancellationToken);
    }

    public virtual Task<long> Remove(IEnumerable<T> entityIn, DeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Remove(entityIn, options, cancellationToken);
    }

    public virtual Task<long> Remove(string id, DeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Remove(id, options, cancellationToken);
    }

    public virtual Task<long> Remove(IEnumerable<string> id, DeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Remove(id, options, cancellationToken);
    }

}