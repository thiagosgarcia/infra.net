using Infra.Net.HttpClientManager;

namespace Template.HttpClient_API.HttpServices;

public interface IEntityServiceHttpClientManager
{
    Task<IEnumerable<EntityServiceHttpClientManager.Entity2>> GetEntities();

    Task<IEnumerable<EntityServiceHttpClientManager.Entity2>> GetEntitiesByFilter(Dictionary<string, string> @params,
        Dictionary<string, string> headers);

    Task<HttpResponseMessage> GetEntitiesFailSafe();
}
public class EntityServiceHttpClientManager : IEntityServiceHttpClientManager
{
    private readonly IHttpClientManager _httpClientManager;

    public EntityServiceHttpClientManager(IHttpClientManager httpClientManager)
    {
        _httpClientManager = httpClientManager;
    }

    public Task<IEnumerable<Entity2>> GetEntities()
        => _httpClientManager.GetAsync<IEnumerable<Entity2>>("ModelEntity");

    public Task<IEnumerable<Entity2>> GetEntitiesByFilter(Dictionary<string, string> @params, Dictionary<string, string> headers)
        => _httpClientManager.GetAsync<IEnumerable<Entity2>>("ModelEntity", @params: @params, headers: headers);


    public Task<HttpResponseMessage> GetEntitiesFailSafe()
        => _httpClientManager.GetAsync("ModelEntity");

    public record Entity2
    {
        public int Codigo { get; init; }
        public string Nome { get; init; }
    }
}