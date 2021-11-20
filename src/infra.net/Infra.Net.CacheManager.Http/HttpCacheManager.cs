using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Infra.Net.Extensions.Extensions;
using Infra.Net.HttpClientManager;

namespace Infra.Net.CacheManager.Http;

public class HttpCacheManager : AbstractCacheManager
{
    private readonly string _configSection;
    private readonly IHttpClientManager _httpClientManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private string _prefixUrl;
    private int _cacheTimeout;

    public static readonly JsonSerializerSettings DefaultJsonProps = new()
    {
        DateTimeZoneHandling = DateTimeZoneHandling.Local,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly Dictionary<string, string> DefaultHeaders = new()
    {
        { "Accept", "text/plain" },
    };

    public HttpCacheManager(IHttpClientManager httpClientManager,
        IConfiguration configuration,
        ILogger logger,
        string configSection)
    {
        _httpClientManager = httpClientManager;
        _configuration = configuration;
        _logger = logger;
        _configSection = configSection;

        Setup();
    }
    private void Log(LogEventLevel level, string msg, Exception ex = null, string args = null)
    {
        if (!(_logger?.IsEnabled(level) ?? false))
            return;

        _logger.Write(level, ex, "HttpCacheManager::{0}:{1}", msg, args);
    }

    public override Task<HttpResponseMessage> Touch(string id, string key,
        IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        Setup();
#endif
        return _httpClientManager.GetAsync(
            resource: $"{_prefixUrl}{Urlfy(id)}/{GenerateKey(key, compositeKey)}",
            headers: GetRequestHeaders(expireTime, idleTime),
            baseUrl: ServerPool.Next().ToString(),
            mediaType: "text/plain",
            cancellationToken: cancellationToken);
    }

    public override async Task<T> GetOrPut<T>(string id, string key, Func<Task<T>> factory, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, bool? performAsync = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await Get(id, key, compositeKey, expireTime, idleTime, jsonProps, cancellationToken);
        if (!string.IsNullOrEmpty(existing))
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(existing, jsonProps ?? DefaultJsonProps);
            }
            catch (Exception ex)
            {
                Log(LogEventLevel.Debug, "Error deserializing object", ex, existing);
            }
        }

        return await RunFactory(id, key, factory, compositeKey, expireTime, idleTime, performAsync, jsonProps, cancellationToken);
    }
    public override async Task<string> GetOrPut(
        string id, string key, Func<Task<string>> factory, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null,
        JsonSerializerSettings jsonProps = null, bool? performAsync = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await Get(id, key, compositeKey, expireTime, idleTime, jsonProps, cancellationToken);
        if (!string.IsNullOrEmpty(existing))
            return existing;

        return await RunFactory(id, key, factory, compositeKey, expireTime, idleTime, performAsync, jsonProps, cancellationToken);
    }

    private async Task<T> RunFactory<T>(string id, string key, Func<Task<T>> factory, IEnumerable<string> compositeKey, TimeSpan? expireTime,
        TimeSpan? idleTime, bool? performAsync, JsonSerializerSettings jsonProps = null, CancellationToken cancellationToken = default)
    {
        var newOne = await factory();
        if (newOne != null)
            if (performAsync.HasValue)
                await PutAsync(id, key, newOne, compositeKey, expireTime, idleTime, performAsync.Value, jsonProps, cancellationToken);
            else
                Put(id, key, newOne, compositeKey, expireTime, idleTime, jsonProps, cancellationToken);

        return newOne;
    }

    public override async Task<string> Get(
        string id, string key, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        Setup();
#endif
        try
        {
            var getTask = _httpClientManager.GetAsync(
                resource: $"{_prefixUrl}{Urlfy(id)}/{GenerateKey(key, compositeKey)}",
                headers: GetRequestHeaders(expireTime, idleTime),
                baseUrl: ServerPool.Next().ToString(),
                mediaType: "text/plain",
                cancellationToken: cancellationToken);

            if (!getTask.Wait(_cacheTimeout * 1000, cancellationToken))
            {
                Log(LogEventLevel.Verbose, "[CACHEMISS]:Timeout");
                return string.Empty;
            }

            var response = await getTask;
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(cancellationToken);
                Log(LogEventLevel.Verbose, "[CACHEHIT]", args: result);
                return result;
            }

            Log(LogEventLevel.Verbose, "[CACHEMISS]:Miss");
        }
        catch (Exception ex)
        {
            _logger?.Verbose(ex, "[CACHEMISS]:Exception");
        }

        return string.Empty;
    }

    public override async Task<T> Get<T>(
        string id, string key, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
        TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var data = await Get(id, key, compositeKey, expireTime, idleTime, jsonProps, cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(data))
                return JsonConvert.DeserializeObject<T>(data, jsonProps ?? DefaultJsonProps);
        }
        catch (Exception ex)
        {
            Log(LogEventLevel.Debug, "Error deserializing object", ex, data);
        }

        return null;
    }

    public override void Put(string id, string key, dynamic value, IEnumerable<string> compositeKey = null, TimeSpan? expireTime = null,
        TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null,
        CancellationToken cancellationToken = default)
    {
        Task.Run(() => PutAsync(id, key, value, compositeKey, expireTime, idleTime, true, jsonProps, cancellationToken));
    }
    public override Task<HttpResponseMessage> PutAsync(
        string id, string key, dynamic value, IEnumerable<string> compositeKey = null,
        TimeSpan? expireTime = null, TimeSpan? idleTime = null, bool? performAsync = null, JsonSerializerSettings jsonProps = null,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        Setup();
#endif

        string jsonValue = null;
        try
        {
            //I have to convert the object now, because text/plain does not convert to json on httpClientManager. But if it's already a string, I'm not going to serialize
            if (!(value is string))
                jsonValue = JsonConvert.SerializeObject(value, jsonProps ?? DefaultJsonProps);
        }
        catch (Exception ex)
        {
            Log(LogEventLevel.Debug, "Error deserializing object", ex, value.ToString());
        }

        return _httpClientManager.PutAsync(
            resource: $"{_prefixUrl}{Urlfy(id)}/{GenerateKey(key, compositeKey)}",
            obj: jsonValue ?? value,
            headers: GetRequestHeaders(expireTime, idleTime, performAsync),
            baseUrl: ServerPool.Next().ToString(),
            mediaType: "text/plain",
            cancellationToken: cancellationToken);
    }
    public override void Delete(string id, string key = null, IEnumerable<string> compositeKey = null, CancellationToken cancellationToken = default)
    {
        Task.Run(() => DeleteAsync(id, key, compositeKey, true, cancellationToken));
    }
    public override Task<HttpResponseMessage> DeleteAsync(string id, string key = null, IEnumerable<string> compositeKey = null, bool? performAsync = null,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        Setup();
#endif
        return _httpClientManager.DeleteAsync(
            resource: $"{_prefixUrl}{Urlfy(id)}{GenerateDeleteKey(key, compositeKey)}",
            headers: GetRequestHeaders(null, null, performAsync),
            baseUrl: ServerPool.Next().ToString(),
            cancellationToken: cancellationToken);
    }

    private Dictionary<string, string> GetRequestHeaders(TimeSpan? expireTime, TimeSpan? idleTime, bool? performAsync = null)
    {
        var headers = DefaultHeaders.CopyStringDictionary();

        if (idleTime != null)
            headers.Add("maxIdleTimeSeconds", idleTime?.TotalSeconds.ToString());
        //It can't have else clause here. I should be able to set both headers, but never none
        if (expireTime != null)
            headers.Add("timeToLiveSeconds", expireTime?.TotalSeconds.ToString());
        else if (idleTime == null) //In case both are null
            headers.Add("timeToLiveSeconds", TimeSpan.FromHours(1).TotalSeconds.ToString());

        if (performAsync.HasValue)
            headers.Add("performAsync", performAsync.Value.ToString());

        Log(LogEventLevel.Verbose, "Headers",
            args: string.Join(", ", headers.Select(x => $"{x.Key}:{x.Value}")));

        return headers;
    }
    private void Setup()
    {
        _prefixUrl = _configuration[$"{_configSection}:PrefixUrl"] ?? "rest/";
        _cacheTimeout = int.Parse(_configuration[$"{_configSection}:CacheTimeout"] ?? "1");

        var servers = _configuration[$"{_configSection}:Servers"] ?? string.Empty;
        var httpSchema = _configuration[$"{_configSection}:Schema"] ?? "http";
#if DEBUG
        if (DefaultHeaders.ContainsKey("Authorization"))
            DefaultHeaders.Remove("Authorization");
#endif
        var authentication = _configuration[$"{_configSection}:Authentication"] ?? string.Empty;
        DefaultHeaders.Add("Authorization", $"Basic {authentication.ToBase64()}");

#if DEBUG
        ServerPool._endpoints.Clear();
#endif
        foreach (var server in servers.Split(';'))
            ServerPool.Add(new Uri($"{httpSchema}://{server.Trim()}"));
    }

}