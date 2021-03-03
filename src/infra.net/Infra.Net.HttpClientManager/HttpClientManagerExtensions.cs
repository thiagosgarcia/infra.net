using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Serilog;
using Infra.Net.Helpers;

namespace Infra.Net.HttpClientManager
{
    public static class HttpClientManagerExtensions
    {
        public static void ConfigureHttpClient(this IServiceCollection services, IConfiguration config, string defaultBaseUrlConfig)
        {
            services.AddHttpClient(string.IsNullOrEmpty(defaultBaseUrlConfig) ? "DefaultBaseUrl" : config[defaultBaseUrlConfig])
                .ConfigurePrimaryHttpMessageHandler(
                    _ => ProxyHelper.CreateHttpClientHandler(config, defaultBaseUrlConfig));
        }

        public static IServiceCollection AddHttpClientManager(this IServiceCollection services, IConfiguration config,
            string defaultBaseUrlConfig = null, JsonSerializerSettings jsonProps = null, bool useHttpClientManagerHandler = true,
            string apiName = null, bool? useDefaultPolicyWrap = null)
        {
            if (useHttpClientManagerHandler)
                ConfigureHttpClient(services, config, defaultBaseUrlConfig);

            if (useDefaultPolicyWrap ?? false)
                DefaultPolicy(services);

            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IHttpClientManager>(x =>
                {
                    var logger = (ILogger)x.GetService(typeof(ILogger));
                    var policyWrap = (IAsyncPolicy)x.GetService(typeof(IAsyncPolicy));
                    return new HttpClientManager(
                        defaultBaseUrlConfig,
                        jsonProps,
                        (IHttpClientFactory)x.GetService(typeof(IHttpClientFactory)),
                        logger,
                        config,
                        (IHttpContextAccessor)x.GetService(typeof(IHttpContextAccessor)),
                        apiName,
                        policyWrap);
                });
        }

        public static IServiceCollection AddHttpClientManagerWithoutLog(this IServiceCollection services, IConfiguration config,
            string defaultBaseUrlConfig = null, JsonSerializerSettings jsonProps = null, bool useHttpClientManagerHandler = true,
            string apiName = null, bool? useDefaultPolicyWrap = null)
        {
            if (useHttpClientManagerHandler)
                ConfigureHttpClient(services, config, defaultBaseUrlConfig);

            if (useDefaultPolicyWrap ?? false)
                DefaultPolicy(services);

            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IHttpClientManager>(x =>
                {
                    var policyWrap = (IAsyncPolicy)x.GetService(typeof(IAsyncPolicy));
                    return new HttpClientManager(
                        defaultBaseUrlConfig,
                        jsonProps,
                        (IHttpClientFactory)x.GetService(typeof(IHttpClientFactory)),
                        null,
                        config,
                        (IHttpContextAccessor)x.GetService(typeof(IHttpContextAccessor)),
                        apiName,
                        policyWrap);
                });
        }

        private static void CustomDefaultPolicy(IServiceCollection services, IAsyncPolicy customPolicyWrap)
        {
            services.AddSingleton<IAsyncPolicy>(customPolicyWrap);
        }

        private static void DefaultPolicy(IServiceCollection services)
        {
            services.AddSingleton<IAsyncPolicy>(x =>
            {
                var logger = (ILogger)x.GetService(typeof(ILogger));

                void OnBreak(Exception exception, TimeSpan timeSpan, Context context) =>
                    logger?.Warning("Entering circuit breaker!");

                void OnReset(Context _) => logger?.Warning("Finishing circuit breaker!");

                var breaker = Policy
                    .Handle<HttpRequestException>()
                    .CircuitBreakerAsync(60, TimeSpan.FromMinutes(1), OnBreak, OnReset);

                var retry =
                    Policy
                        .Handle<HttpRequestException>()
                        .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(1.5, i)),
                            (exception, span, i, context) =>
                            {
                                logger?.Warning(exception, "Exception raised, retrying in {0}s...", span.TotalSeconds);
                            });

                return Policy.WrapAsync(retry, breaker);
            });
        }

    }
}
