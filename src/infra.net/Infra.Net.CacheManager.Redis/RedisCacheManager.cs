using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;

namespace Infra.Net.CacheManager.Redis;

public class RedisCacheManager : IRedisCacheManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly string _configSection;
    private static ConnectionMultiplexer _multiplexer;
    private TimeSpan _defaultCacheExpirationMinutes;
    private static IDatabase Database => _multiplexer.GetDatabase();

    public RedisCacheManager(
        IConfiguration configuration,
        ILogger logger,
        string configSection)
    {
        _configuration = configuration;
        _logger = logger;
        _configSection = configSection;

        Setup();
    }

    private void Setup()
    {
        var connectionString = _configuration[$"{_configSection}:ConnectionString"] ?? string.Empty;
        var logWriter = new StringWriter();
        _multiplexer = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(connectionString), logWriter);
#pragma warning disable CS4014
        HandleConnectionLogs(logWriter);
#pragma warning restore CS4014

        SetupVariables();
    }

    private void SetupVariables()
    {
        _defaultCacheExpirationMinutes = TimeSpan.FromMinutes(int.Parse(_configuration[$"{_configSection}:DefaultCacheExpirationMinutes"] ?? "60"));
    }

    private async Task HandleConnectionLogs(StringWriter writer)
    {
        using var sr = new StringReader(writer.ToString());
        string line;
        while ((line = await sr.ReadLineAsync()) != null)
            _logger.Information(line);
    }

    public async Task TouchAsync(string key, TimeSpan? expireTime = null)
    {
#if DEBUG
        SetupVariables();
#endif
        await Database.KeyTouchAsync(key);
        await Database.KeyExpireAsync(key, expireTime ?? _defaultCacheExpirationMinutes);
    }

    public async Task<T> GetOrPutAsync<T>(string key, Func<Task<T>> factory,
        TimeSpan? expireTime = null, bool replaceExisting = true) where T : class
    {
        var current = await GetAsync<T>(key);
        if (current != null)
            return current;

#if DEBUG
        SetupVariables();
#endif
        var value = await factory();
        await PutAsync(key, value, expireTime ?? _defaultCacheExpirationMinutes, replaceExisting);
        return value;
    }

    public async Task<T> GetAsync<T>(string key) where T : class
    {
        var current = await Database.StringGetAsync(key);
        if (current.HasValue)
            return JsonConvert.DeserializeObject<T>(current.ToString());

        return null;
    }

    public Task PutAsync<T>(string key, T value, TimeSpan? expireTime = null, bool replaceExisting = true) where T : class
    {
#if DEBUG
        SetupVariables();
#endif
        return Database.StringSetAsync(key, JsonConvert.SerializeObject(value), expireTime ?? _defaultCacheExpirationMinutes, replaceExisting ? When.Always : When.NotExists);
    }

    public Task DeleteAsync(string key)
    {
        return Database.KeyDeleteAsync(key);
    }

}
