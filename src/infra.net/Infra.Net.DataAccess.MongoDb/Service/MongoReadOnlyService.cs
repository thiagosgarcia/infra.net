using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Infra.Net.DataAccess.MongoDb.Repository;

namespace Infra.Net.DataAccess.MongoDb.Service;

public class MongoReadOnlyService<T> : IMongoReadOnlyService<T> where T : MongoEntity
{
    protected readonly IMongoRepository<T> _repository;

    public MongoReadOnlyService(IMongoRepository<T> repository)
    {
        _repository = repository;
    }

    public IMongoCollection<T> Items => _repository.Items;

    public virtual async Task<bool> Exists(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        var data = await _repository.Get(filterFunc, options, cancellationToken);
        return await data.MoveNextAsync(cancellationToken) && data.Current.Any();
    }
    public virtual async Task<long> Count(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        var data = await _repository.Get(filterFunc, options, cancellationToken);
        var count = 0L;

        while (await data.MoveNextAsync(cancellationToken))
            count += data.Current?.Count() ?? 0;

        return count;
    }
    public virtual async Task<T> GetFirst(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        var data = await _repository.Get(filterFunc, options, cancellationToken);

        if (await data.MoveNextAsync(cancellationToken))
            return data.Current.First();

        throw new InvalidOperationException("Sequence contains no elements");
    }
    public virtual async Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        var data = await _repository.Get(filterFunc, options, cancellationToken);

        if (await data.MoveNextAsync(cancellationToken))
            return data.Current.FirstOrDefault();

        return null;
    }
    public virtual async Task<IEnumerable<T>> Get(int page = 0, int perPage = 10, Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        var data = await _repository.Get(filterFunc, options, cancellationToken);
        var result = new List<T>();

        while (await data.MoveNextAsync(cancellationToken) && result.Count < perPage)
        {
            var inter = data.Current?.Skip(perPage * page).Take(perPage) ?? Array.Empty<T>();
            result.AddRange(inter);
        }

        return result;
    }
    public virtual async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filterFunc, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        var data = await _repository.Get(filterFunc, options, cancellationToken);
        var result = new List<T>();

        while (await data.MoveNextAsync(cancellationToken))
        {
            var inter = data.Current ?? Array.Empty<T>();
            result.AddRange(inter);
        }

        return result;
    }

    public virtual Task<T> Get(string id, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        return _repository.Get(id, options, cancellationToken);
    }
}