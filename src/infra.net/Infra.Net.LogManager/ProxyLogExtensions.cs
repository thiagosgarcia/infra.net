using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;

namespace Infra.Net.LogManager
{
    public static class ProxyLogExtensions
    {
        public static IServiceCollection AddScopedWithLog<TInterface, TService>(this IServiceCollection services, LogEventLevel logLevel = LogEventLevel.Information) 
            where TInterface : class where TService : class, TInterface
        {
            return
                services.AddScoped<TService>()
                    .AddScoped(x => 
                        ProxyLogManager<TInterface>.Create(x.GetService<TService>(), x, logLevel));
        }
        public static IServiceCollection AddTransientWithLog<TInterface, TService>(this IServiceCollection services, LogEventLevel logLevel = LogEventLevel.Information)
            where TInterface : class where TService : class, TInterface
        {
            return
                services.AddTransient<TService>()
                    .AddTransient(x => 
                        ProxyLogManager<TInterface>.Create(x.GetService<TService>(), x, logLevel));
        }
        public static IServiceCollection AddSingletonWithLog<TInterface, TService>(this IServiceCollection services, LogEventLevel logLevel = LogEventLevel.Information)
            where TInterface : class where TService : class, TInterface
        {
            return
                services.AddSingleton<TService>()
                    .AddSingleton(x => 
                        ProxyLogManager<TInterface>.Create(x.GetService<TService>(), x, logLevel));
        }
    }
}
