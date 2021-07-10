using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infra.Net.LogManager.WebExtensions
{
    public static class WebLogManagerExtensions
    {
        public static IServiceCollection AddWebLogManager(this IServiceCollection services)
        {
            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<ILogger>(x => new LoggerConfiguration()
                .ReadFrom.Configuration((IConfiguration)x.GetService(typeof(IConfiguration)))
                .Enrich.With(new CorrelationIdEnricher((IHttpContextAccessor)x.GetService(typeof(IHttpContextAccessor))))
                .CreateLogger());
        }
    }
}