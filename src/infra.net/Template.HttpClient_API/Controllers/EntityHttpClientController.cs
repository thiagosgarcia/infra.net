using Microsoft.AspNetCore.Mvc;
using Template.HttpClient_API.HttpServices;
using Infra.Net.LogManager.WebExtensions.Attributes;
using Infra.Net.CacheManager.Http;
using static Template.HttpClient_API.HttpServices.EntityServiceHttpClientManager;

namespace Template.HttpClient_API.Controllers;

[HandleException]
[ApiController]
[Route("[controller]")]
public class EntityHttpClientController : ControllerBase
{
    private readonly IEntityServiceHttpClientManager _EntityService;

    public EntityHttpClientController(IEntityServiceHttpClientManager EntityService)
    {
        _EntityService = EntityService;
    }

    [CachedOperation]
    [HttpGet("ModelEntity")]
    public Task<IEnumerable<Entity2>> Get()
    {
        return _EntityService.GetEntities();
    }

    /// <summary>
    /// Exemplo sem cache
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    [HttpGet("Entity2")]
    public Task<IEnumerable<Entity2>> Get(string filter, string orderBy)
    {
        var @params = new Dictionary<string, string>()
        {
            {nameof(filter), filter},
            {nameof(orderBy), orderBy}
        };
        var headers = new Dictionary<string, string>()
        {
            {nameof(filter), filter},
            {nameof(orderBy), orderBy}
        };
        return _EntityService.GetEntitiesByFilter(@params, @headers);
    }

    /// <summary>
    /// Exemplo com cache
    /// </summary>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /EntityHttpClient/ModelEntity/Cache?filter=itau
    /// 
    /// </remarks>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    [CachedOperation]
    [HttpGet("ModelEntity/Cache")]
    public Task<IEnumerable<Entity2>> Cache(string filter, string orderBy)
    {
        var @params = new Dictionary<string, string>()
        {
            {nameof(filter), filter},
            {nameof(orderBy), orderBy}
        };
        var headers = new Dictionary<string, string>()
        {
            {nameof(filter), filter},
            {nameof(orderBy), orderBy}
        };
        return _EntityService.GetEntitiesByFilter(@params, @headers);
    }
}