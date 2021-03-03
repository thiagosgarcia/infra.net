using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using Serilog;
using Template.HttpClient_API.HttpServices;
using Infra.Net.Extensions.ExceptionHandlers;
using Infra.Net.LogManager.WebExtensions.Attributes;
using Infra.Net.HttpClientManager;

namespace Template.HttpClient_API.Controllers
{
    [HandleException]
    [ApiController]
    [Route("[controller]")]
    public class PollyController : ControllerBase
    {
        private readonly IHttpClientManager _clientManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        string selfErrorUrl;
        string selfSlowUrl;
        string selfGetUrl;
        string realUrl;
        string fakeUrl;

        public PollyController(IHttpClientManager clientManager, IConfiguration configuration, ILogger logger)
        {
            _clientManager = clientManager;
            _configuration = configuration;
            _logger = logger;

            realUrl = _configuration["FakeApi1"];
            fakeUrl = realUrl + "WRONG";

            var selfUrl = _configuration["SelfUrl"];

            selfErrorUrl = $"{selfUrl}ModelEntity";
            selfSlowUrl = $"{selfUrl}ModelEntity/Lento";
            selfGetUrl = $"{selfUrl}Entity2";

        }

        [HttpGet("Error")]
        public async Task<IEnumerable<Entity>> Error()
        {
            Action<Exception, TimeSpan, Context> onBreak = (exception, timespan, context) =>
            {
                _logger.Warning("Entering circuit breaker!");
            };
            Action<Context> onReset = context =>
            {
                _logger.Warning("Finishing circuit breaker!");
            };
            AsyncCircuitBreakerPolicy breaker = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(10), onBreak, onReset);

            AsyncRetryPolicy timeoutPolicy =
                Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(4, i => TimeSpan.FromSeconds(Math.Pow(1.5, i)),
                        (exception, span, i, context) =>
                        {
                            _logger.Error(exception, "Exception raised, retrying in {0}s...", span.TotalSeconds);
                            //_logger.Error(exception, "After {0} attempts in {1}s, the API is still getting errors. Failing...", i, span.TotalSeconds);
                        });

            AsyncPolicyWrap policyWrap = Policy.WrapAsync(timeoutPolicy, breaker);

            var dict = new Dictionary<string, string>()
            {
                {"waitTime", 0.ToString()}
            };
            var result = await policyWrap.ExecuteAsync(async () =>
                await _clientManager.GetAsync<IEnumerable<Entity>>(
                    new Random().Next(0, 50) > 0 ? selfErrorUrl : selfSlowUrl, dict, baseUrl: ""));
            return result;
        }

        [HttpGet("Error/HttpClientManager/CustomWrapper")]
        public async Task<IEnumerable<Entity>> ErrorHttpClientManagerCustomWrapper()
        {
            bool authorized = false;
            Action<Exception, TimeSpan, Context> onBreak = (exception, timespan, context) =>
            {
                _logger.Warning("Entering circuit breaker!");
            };
            Action<Context> onReset = context =>
            {
                _logger.Warning("Finishing circuit breaker!");
            };
            AsyncCircuitBreakerPolicy breaker = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(60, TimeSpan.FromSeconds(10), onBreak, onReset);

            AsyncRetryPolicy retryPolicy =
                Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(Math.Pow(1.5, i)),
                        (exception, span, i, context) =>
                        {
                            _logger.Warning(exception, "Exception raised, retrying in {0}s...", span.TotalSeconds);
                            //_logger.Error(exception, "After {0} attempts in {1}s, the API is still getting errors. Failing...", i, span.TotalSeconds);
                        });
            
            AsyncRetryPolicy authPolicy =
                Policy
                    .Handle<AuthorizationException>()
                    .WaitAndRetryAsync(4, i => TimeSpan.FromSeconds(Math.Pow(1.5, i)),
                        async (exception, span, i, context) =>
                        {
                            _logger.Warning(exception, "Simulating authorization...");
                            await _clientManager.GetAsync<IEnumerable<Entity>>(selfGetUrl, baseUrl: "");
                            authorized = true;
                            //_logger.Error(exception, "After {0} attempts in {1}s, the API is still getting errors. Failing...", i, span.TotalSeconds);
                        });

            AsyncPolicyWrap policyWrap = Policy.WrapAsync(retryPolicy, authPolicy, breaker);

            var dict = new Dictionary<string, string>()
            {
                {"waitTime", 0.ToString()}
            };
            //run from inside httpClientManager
            //var result = await _clientManager.GetAsync<IEnumerable<ModelEntity>>(
            //        new Random().Next(0, 50) > 20 ? selfErrorUrl : selfSlowUrl, dict, baseUrl: "", policyWrap: policyWrap);
            
            //run outside httClientManager
            var result = await policyWrap.ExecuteAsync(async () =>
            {
                if (new Random().Next(0, 2) > 0 && !authorized)
                    throw new AuthorizationException("login", "request", "", "");

               return await _clientManager.GetAsync<IEnumerable<Entity>>(
                    new Random().Next(0, 50) > 30 ? selfErrorUrl : selfSlowUrl, dict, baseUrl: "");
            });

            return result;
        }

        [HttpGet("Error/HttpClientManager/DefaultWrapper")]
        public async Task<IEnumerable<Entity>> ErrorHttpClientManagerDefaultWrapper()
        {
            var dict = new Dictionary<string, string>()
            {
                {"waitTime", 0.ToString()}
            };
            var result = await _clientManager.GetAsync<IEnumerable<Entity>>(
                    new Random().Next(0, 50) > 0 ? selfErrorUrl : selfSlowUrl, dict, baseUrl: "");
            return result;
        }

        [HttpGet("Slow")]
        public async Task<IEnumerable<Entity>> Get()
        {
            AsyncPolicy<IEnumerable<Entity>> timeoutPolicy = Policy.TimeoutAsync<IEnumerable<Entity>>(
                TimeSpan.FromSeconds(1),
                async (_, _, _) => await Task.Run(() => _logger.Warning("Timeout!")));

            var waitTime = new Random().Next(10, 12);
            var dict = new Dictionary<string, string>()
                {
                    {nameof(waitTime), waitTime.ToString()}
                };

            var result = await timeoutPolicy.ExecuteAsync(
                async (token) => await _clientManager.GetAsync<IEnumerable<Entity>>(selfSlowUrl, dict, baseUrl: "", cancellationToken: token),
                new CancellationToken());
            return result;
        }

        [HttpGet("SlowFromServices")]
        public async Task<IEnumerable<Entity>> Get([FromServices] IAsyncPolicy<IEnumerable<Entity>> policy)
        {
            var waitTime = new Random().Next(10, 12);
            var dict = new Dictionary<string, string>()
                {
                    {nameof(waitTime), waitTime.ToString()}
                };

            var result = await policy.ExecuteAsync(
                async (token) => await _clientManager.GetAsync<IEnumerable<Entity>>(selfSlowUrl, dict, baseUrl: "", cancellationToken: token),
                new CancellationToken());
            return result;
        }
    }

    public class Entity
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }
}
