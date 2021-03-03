using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Infra.Net.DataAccess.MongoDb.Repository;
using Infra.Net.DataAccess.MongoDb.Service;

namespace Infra.Net.DataAccess.MongoDb.WebHelpers
{
    public interface IMongoHttpService<T> : IMongoService<T> where T : MongoEntity
    {
        Task<IEnumerable<T>> Get(int page = 0, int perPage = 10, Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default);
    }

    public class MongoHttpService<T> : MongoService<T>, IMongoHttpService<T> where T : MongoEntity
    {
        private readonly IMongoRepository<T> _repository;
        private readonly IMongoHttpReadOnlyService<T> _baseService;

        public MongoHttpService(IMongoRepository<T> repository, IMongoHttpReadOnlyService<T> baseService) : base(repository)
        {
            _repository = repository;
            _baseService = baseService;
            _baseService = baseService;
        }

        public override Task<IEnumerable<T>> Get(int page = 0, int perPage = 10, Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null,
            CancellationToken cancellationToken = default)
                => _baseService.Get(page, perPage, filterFunc, options, cancellationToken);
    }

    public interface IMongoHttpReadOnlyService<T> : IMongoReadOnlyService<T> where T : MongoEntity
    {
        Task<IEnumerable<T>> Get(int page = 0, int perPage = 10, Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default);
    }

    public class MongoHttpReadOnlyService<T> : MongoReadOnlyService<T>, IMongoHttpReadOnlyService<T> where T : MongoEntity
    {
        protected readonly IMongoRepository<T> _repository;
        private readonly IHttpContextAccessor _accessor;

        public MongoHttpReadOnlyService(IMongoRepository<T> repository, IHttpContextAccessor accessor) : base(repository)
        {
            _repository = repository;
            _accessor = accessor;
        }


        public override async Task<IEnumerable<T>> Get(int page = 0, int perPage = 10, Expression<Func<T, bool>> filterFunc = null, FindOptions<T> options = null, CancellationToken cancellationToken = default)
        {
            var data = await _repository.Get(filterFunc, options, cancellationToken);
            var result = new List<T>();
            var count = 0L;

            while (await data.MoveNextAsync(cancellationToken))
            {
                if (result.Count < perPage)
                {
                    var inter = data.Current?.Skip(perPage * page).Take(perPage) ?? Array.Empty<T>();
                    result.AddRange(inter);
                }
                count += data.Current?.Count() ?? 0;
            }

            _accessor.HttpContext.Response.Headers.TryAdd("query-ItemCount", count.ToString());
            _accessor.HttpContext.Response.Headers.TryAdd("query-Page", page.ToString());
            _accessor.HttpContext.Response.Headers.TryAdd("query-PerPage", perPage.ToString());
            _accessor.HttpContext.Response.Headers.TryAdd("query-ItemsInPage", result.Count.ToString());

            return result;
        }
    }
}
