using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace Infra.Net.CacheManager.Memcached
{
    public class MemcachedManager : AbstractBinaryCacheManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IMemcachedClient _client;
        private int _cacheTimeout;
        public bool TraceEnabled { get; set; }
        public string TraceType { get; set; }

        private readonly string _configSection;
        //private MemcachedClient _client;

        public static readonly JsonSerializerSettings DefaultJsonProps = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };


        public MemcachedManager(
            IConfiguration configuration,
            ILogger logger,
            IMemcachedClient client,
            string configSection)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
            _configSection = configSection;

            Setup();
        }

        private void Setup()
        {
            _cacheTimeout = int.Parse(_configuration[$"{_configSection}:CacheTimeout"] ?? "1");
            TraceEnabled = Convert.ToBoolean(_configuration["Logging:ElasticTraceEnabled"] ?? "false") &&
                           Convert.ToBoolean(_configuration["Logging:ElasticCacheTraceEnabled"] ?? "false");
            TraceType = _configuration["Logging:ElasticTraceType"] ?? ApiConstants.TypeRequest;
        }
        private TimeSpan GetExpiration(TimeSpan? expireTime, TimeSpan? idleTime) => expireTime ?? idleTime ?? TimeSpan.FromHours(1);
        private void Log(LogEventLevel level, string msg, Exception ex = null, string args = null)
        {
            if (!(_logger?.IsEnabled(level) ?? false))
                return;

            _logger.Write(level, ex, "MemcachedManager::{0}:{1}", msg, args);
        }

        public override async Task<bool> Touch(string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null, CancellationToken cancellationToken = default)
        {
#if DEBUG
            Setup();
#endif
            async Task<bool> DoTouch()
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var touchTask = _client.TouchAsync(GenerateKey(key, compositeKey), GetExpiration(expireTime, idleTime));

                    if (!touchTask.Wait(_cacheTimeout * 1000, cancellationToken))
                    {
                        Log(LogEventLevel.Verbose, "[CACHEMISS]:Timeout");
                        return false;
                    }

                    var result = await touchTask;

                    if (result.Success)
                    {
                        Log(LogEventLevel.Verbose, "[CACHEHIT]");
                        return result.Success;
                    }

                    Log(LogEventLevel.Verbose, "[CACHEMISS]:Miss");
                }
                catch (Exception ex)
                {
                    _logger?.Verbose(ex, "[CACHEMISS]:Exception");
                }

                return false;
            }


            if (!TraceEnabled || Agent.Tracer?.CurrentTransaction == null)
                return await DoTouch();

            return await Agent.Tracer
                .CurrentTransaction.CaptureSpan(
                    $"MemcachedManager::Touch", TraceType, async (t) =>
                        await DoTouch(), ApiConstants.SubtypeHttp, ApiConstants.ActionExec);
        }

        public override bool Delete(string id, string key = null, IEnumerable<string> compositeKey = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => DeleteAsync(id, key, compositeKey, cancellationToken), cancellationToken).Result;
        }

        public override Task<bool> DeleteAsync(string id, string key = null, IEnumerable<string> compositeKey = null,
            CancellationToken cancellationToken = default)
        {
#if DEBUG
            Setup();
#endif
            cancellationToken.ThrowIfCancellationRequested();
            return _client.RemoveAsync(GenerateDeleteKey(key, compositeKey));
        }

        public override Task<bool> PutAsync(string id, string key, dynamic value, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null, CancellationToken cancellationToken = default)
        {
#if DEBUG
            Setup();
#endif

            Task<bool> DoPut()
            {
                cancellationToken.ThrowIfCancellationRequested();
                return _client.StoreAsync(StoreMode.Set, GenerateKey(key, compositeKey), value,
                    GetExpiration(expireTime, idleTime));
            }

            if (!TraceEnabled || Agent.Tracer?.CurrentTransaction == null)
                return DoPut();

            return Agent.Tracer
                .CurrentTransaction.CaptureSpan(
                    $"MemcachedManager::Put", TraceType, async (t) =>
                        await DoPut(), ApiConstants.SubtypeHttp, ApiConstants.ActionExec);
        }


        public override bool Put(string id, string key, dynamic value, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => PutAsync(id, key, value, compositeKey, expireTime, idleTime, cancellationToken), cancellationToken).Result;
        }

        public override async Task<T> Get<T>(string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null, CancellationToken cancellationToken = default) where T : class
        {
#if DEBUG
            Setup();
#endif
            async Task<T> DoGet()
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var getTask = _client.GetAsync<T>(GenerateKey(key, compositeKey));

                    if (!getTask.Wait(_cacheTimeout * 1000, cancellationToken))
                    {
                        Log(LogEventLevel.Verbose, "[CACHEMISS]:Timeout");
                        return null;
                    }

                    var result = await getTask;

                    if (result.Success)
                    {
                        Log(LogEventLevel.Verbose, "[CACHEHIT]");
                        return result.Value;
                    }

                    Log(LogEventLevel.Verbose, "[CACHEMISS]:Miss");
                }
                catch (Exception ex)
                {
                    _logger?.Verbose(ex, "[CACHEMISS]:Exception");
                }

                return null;
            }

            if (!TraceEnabled || Agent.Tracer?.CurrentTransaction == null)
                return await DoGet();

            return await Agent.Tracer
                .CurrentTransaction.CaptureSpan(
                    $"MemcachedManager::Get", TraceType, async (t) =>
                        await DoGet(), ApiConstants.SubtypeHttp, ApiConstants.ActionExec);
        }
        public override async Task<T> GetOrPut<T>(string id, string key, Func<Task<T>> factory, IEnumerable<string> compositeKey, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null, CancellationToken cancellationToken = default) where T : class
        {
            var value = await Get<T>(id, key, compositeKey, expireTime, idleTime, cancellationToken);
            if (value != null)
                return value;

            var newOne = await factory();
            await PutAsync(id, key, newOne, compositeKey, expireTime, idleTime, cancellationToken);
            return newOne;
        }
    }
}
