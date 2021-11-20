using Newtonsoft.Json;
using Infra.Net.Extensions.ExceptionHandlers;
using Infra.Net.Extensions.Extensions;

namespace Template.HttpClient_API.HttpServices;

public class EntityService
{
    // _httpClient isn't exposed publicly
    private readonly HttpClient _httpClient;

    public EntityService(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<IEnumerable<ModelEntity>> GetEntities()
    {
        //Exemplo de erro tratado pelo HandleExceptionAttribute
        throw new AuthorizationException("teste", "path", "value", "controller");
    }

    public async Task<IEnumerable<ModelEntity>> GetEntities2()
    {
        //Exemplo de erro tratado pelo HandleExceptionAttribute
        var response = await _httpClient.GetAsync("ModelEntity");

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject
            <IEnumerable<ModelEntity>>(responseString, HttpClientConfiguration.JsonProps);
    }

    public async Task<IEnumerable<ModelEntity>> GetEntitiesByFilter(Dictionary<string, string> @params, Dictionary<string, string> headers)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"ModelEntity{@params.BuildResource()}");
        request.Headers.Add(headers);

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject
            <IEnumerable<ModelEntity>>(responseString, HttpClientConfiguration.JsonProps);
    }

    public async Task<HttpResponseMessage> GetEntitiesFailSafe()
    {
        return await _httpClient.GetAsync("ModelEntity");
    }
}

public record ModelEntity
{
    public int Codigo { get; init; }
    public string Nome { get; init; }
}