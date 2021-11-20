using Enyim.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infra.Net.CacheManager.Memcached;

public static class HttpCacheManagerExtensions
{
    public static IServiceCollection AddMemcachedManager(this IServiceCollection services,
        string configSection = "Memcached")
    {
        return services.AddEnyimMemcached()
            .AddSingleton<IBinaryCacheManager>(x =>
                new MemcachedManager(x.GetService<IConfiguration>(),
                    x.GetService<ILogger>(),
                    x.GetService<IMemcachedClient>(),
                    configSection));
    }

    public static IServiceCollection AddMemcachedManagerWithoutLog(this IServiceCollection services,
        string configSection = "Memcached")
    {
        return services.AddEnyimMemcached()
            .AddSingleton<IBinaryCacheManager>(x =>
                new MemcachedManager(x.GetService<IConfiguration>(),
                    null,
                    x.GetService<IMemcachedClient>(),
                    configSection));
    }
}