using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Infra.Net.DataAccess.MongoDb.Repository
{
    public class MongoRepository<T> : MongoReadOnlyRepository<T>, IMongoRepository<T> where T : MongoEntity
    {
        public MongoRepository(string connectionString, string dbName, string collection, IConfiguration config)
            : base(connectionString, dbName, collection, config)
        {
        }

        public virtual async Task<T> Create(T entity, InsertOneOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new InsertOneOptions();
            await Items.InsertOneAsync(entity, options, cancellationToken);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> Create(IEnumerable<T> list, InsertManyOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new InsertManyOptions();
            var l = list.ToList();
            await Items.InsertManyAsync(l, options, cancellationToken);
            return l;
        }

        public virtual async Task<T> Update(string id, T entityIn, ReplaceOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new ReplaceOptions();
            var result
                = await Items.ReplaceOneAsync(x => x.Id == id, entityIn, options, cancellationToken);
            return result.ModifiedCount > 0 ? await Get(id, null, cancellationToken) : null;
        }

        public virtual async Task<long> Remove(T entity, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new DeleteOptions();
            var result = await Items.DeleteOneAsync(x => x.Id == entity.Id, options, cancellationToken);
            return result.DeletedCount;
        }

        public virtual async Task<long> Remove(IEnumerable<T> list, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new DeleteOptions();
            var builder = new FilterDefinitionBuilder<T>();
            var filter = builder.In(x => x.Id, list.Select(a => a.Id));
            var result = await Items.DeleteManyAsync(filter, options, cancellationToken);
            return result.DeletedCount;
        }

        public virtual async Task<long> Remove(string id, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new DeleteOptions();
            var result = await Items.DeleteOneAsync(x => x.Id == id, options, cancellationToken);
            return result.DeletedCount;
        }

        public virtual async Task<long> Remove(IEnumerable<string> list, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new DeleteOptions();
            var builder = new FilterDefinitionBuilder<T>();
            var filter = builder.In(x => x.Id, list);
            var result = await Items.DeleteManyAsync(filter, options, cancellationToken);
            return result.DeletedCount;
        }

    }
}