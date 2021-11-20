using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Infra.Net.HttpClientManager;

namespace Infra.Net.CacheManager.Http;

public static class HttpCacheManagerExtensions
{
    public static IServiceCollection AddHttpCacheManager(this IServiceCollection services,
        string configSection = "HttpCacheManager")
        => services.AddSingleton<ICacheManager>(x =>
            new HttpCacheManager(
                x.GetService<IHttpClientManager>()
                , x.GetService<IConfiguration>(),
                x.GetService<ILogger>(),
                configSection));

    public static IServiceCollection AddHttpCacheManagerWithoutLog(this IServiceCollection services,
        string configSection = "HttpCacheManager")
        => services.AddSingleton<ICacheManager>(x =>
            new HttpCacheManager(
                x.GetService<IHttpClientManager>()
                , x.GetService<IConfiguration>(),
                null,
                configSection));
}