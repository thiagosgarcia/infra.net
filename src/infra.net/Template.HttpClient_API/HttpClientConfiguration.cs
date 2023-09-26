using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;
using Polly;
using Template.HttpClient_API.HttpServices;
using Infra.Net.LogManager;
using Infra.Net.LogManager.WebExtensions;
using Infra.Net.CacheManager.Http;
using Infra.Net.HttpClientManager;
using Template.HttpClient_API.Controllers;

namespace Template.HttpClient_API;

public static class HttpClientConfiguration
{
    public static readonly JsonSerializerSettings JsonProps = new()
    {
        DateTimeZoneHandling = DateTimeZoneHandling.Local,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    public static void Configure(IServiceCollection services, IConfiguration config)
    {
        ConfigureHeaders(services);
        ConfigureClients(services, config);
    }

    private static void ConfigureClients(IServiceCollection services, IConfiguration config)
    {
        //services.AddScoped<EntityServiceHttpClientManager>();
        services.AddScopedWithLog<IEntityServiceHttpClientManager, EntityServiceHttpClientManager>();

        //Para o HttpClientManager, utilizar esta única chamada...
        //services.AddHttpClientManager(config, "FakeApi1");

        //...ou a combinação das duas abaixo...
        //services.AddHttpClientManager(config, "FakeApi1", useHttpClientManagerHandler: false);
        //services.ConfigureHttpClient(config, "FakeApi1");

        //...com políticas de repetição de chamadas...
        services.AddHttpClientManager(config, "FakeApi1", useDefaultPolicyWrap: true);

        //Con o ConfigureHttpClient é possível definir Handlers para qualquer endpoint, mesmo que não seja o default do HttpClientManager
        //services.ConfigureHttpClient(config, "FakeApi1");
        //services.ConfigureHttpClient(config, "FakeApi2");

        services.AddHttpClient<EntityService>(c =>
        {
            c.BaseAddress = new Uri(config["FakeApi1"]);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            c.DefaultRequestHeaders.Add("User-Agent", "HttpClient_Api");
        });
            
        services.AddWebLogManager();
        services.AddHttpCacheManager();

        //Testing custom policiesvia DI
        services.AddSingleton<IAsyncPolicy<IEnumerable<Entity>>>(
            Policy.TimeoutAsync<IEnumerable<Entity>>(
                TimeSpan.FromSeconds(5)));
    }

    private static void ConfigureHeaders(IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
        });

        services.AddHeaderPropagation(options =>
        {
            options.Headers.Add("Authorization");
            options.Headers.Add("CorrelationId");
            options.Headers.Add("LoginGuid");
            options.Headers.Add("BusinessValue");
            options.Headers.Add("ClientIp");
            options.Headers.Add("X-Forwarded-For");
            options.Headers.Add("PortalName");
            options.Headers.Add("AuthToken");
        });
    }
}