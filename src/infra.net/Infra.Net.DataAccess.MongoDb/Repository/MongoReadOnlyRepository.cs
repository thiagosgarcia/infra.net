using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Infra.Net.DataAccess.MongoDb.Repository;

public class MongoReadOnlyRepository<T> : IMongoReadOnlyRepository<T> where T : MongoEntity
{
    public MongoReadOnlyRepository(string connectionString, string dbName, string collection, IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString(connectionString));
        var database = client.GetDatabase(dbName);
        Items = database.GetCollection<T>(collection);
    }
    public IMongoCollection<T> Items { get; }

    public virtual Task<IAsyncCursor<T>> Get(Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        options ??= new FindOptions<T>();
        if (filterFunc == null)
            return Items.FindAsync(x => true, options, cancellationToken);

        return Items.FindAsync(filterFunc, options, cancellationToken);
    }

    public virtual async Task<T> Get(string id, FindOptions<T> options = null, CancellationToken cancellationToken = default)
    {
        options ??= new FindOptions<T>();
        return await (await Items.FindAsync(Builders<T>.Filter.Eq(x => x.Id, id), options, cancellationToken)).FirstOrDefaultAsync(cancellationToken);
    }
}