### INFRA(structure).NET(5)

# Infra.NET

The goal of this project is to have a swiss knife for any .NETCore project that helps you to keep you head on business, and forget about monitoring, logging, auditing, managing HttpClients, etc. It was initially designed for web apps, but it works with any flavor.

To summarize: a bunch of helpers and extensions that makes me a little more happy in day by day work. Hope it does the same to you =)

## Infra.Net.HttpClientManager

My favorite son.

Abstractions for HttpClient and HttpClientFactory using Polly and embeded moritoring with ElasticAPM. Instead of all the work dealing with Factories and HttpClient's different ways to be used, this assures that you'll only need one line of code for most cases.

Usage:

``` cs
    // Startup.cs
    services.AddHttpClientManager(Configuration);

    // Example - service calls using HttpClient
    var resource = "api/v1/myRestApi/Entity";
    var baseUrl = "http://mycoolapp.com";

    //HTTP GET
    var entities = await _httpClientManager.GetAsync<IEnumerable<Entity>>(resource,  @params, headers, baseUrl);
    //HTTP POST
    var payload = "{ \"name\": \"C3PO\", \"code\": \"XPTO\" }";
    var addedEntity = await _httpClientManager.PostAsync<IEnumerable<Entity>>(resource, payload,  @params, headers, baseUrl);
    //HTTP DELETE
    var httpResult = await _httpClientManager.PostAsync(resource,  @params, headers, baseUrl);
```

More details, docs and how to take advantage of Polly and ElasticAPM soon.

## Infra.Net.CacheManager

My third favorite son.

This is a extensible cache driver for dealing easily with caching. (I know is not THAT extensible, but I'll be working on it).

There's `Infra.Net.CacheManager.Http` which is great for using with `DataGrid` (production tested). It uses `Infra.Net.HttpClientManager` to handle these requests.

And also `Infra.Net.CacheManager.Memcached` for any `Memcached` server, so far I used it with the very `Memcached` in containers.

Following the idea of `CacheManager` it helps you to standardize calls for cache and focus on the business.

``` cs
  var entry = await _cacheManager.Get<string>(cacheId, setupKey, cancellationToken: cancellationToken);

  var result = await _cacheManager.GetOrPut<ObjectResult>(_cacheId, key,
                    async () =>
                    {
                        // do some stuff
                    }, @params, expireTime, idleTime, performAsync: true);
```

And, of course, if you still think this is not woth it, there's the beloved `CachedOperationAttribute` that handles all that stuff directly in you controller. There's still some work to do, but it already fits most cases.

``` cs
    [CachedOperation]
    [HttpGet("ModelEntity")]
    public Task<IEnumerable<Entity>> Get()
    {
        return _EntityService.GetEntities();
    }
```

## Infra.Net.LogManager

This is a cool - and production ready - abstraction of Serilog's logging management, that handles automatic tracing and auditing generating logs based on dependency injection.

``` cs
    //Startup.cs
    //Use DI like this:
    services.AddScopedWithLog<Interface, Service>();
    services.AddSingletonWithLog<Interface, Service>();

    // and configure some stuff:
    // appsettings.json
    //[...] SerilogSettings
    {
        "Name": "Console",
        "Args": {
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.ffffff zzz} [{Level:u3}] {CorrelationId} {ElasticApmTraceId} {ElasticApmTransactionId} {Message}{NewLine}{Exception}" // Template to get all the traces
        }
    }
    //[...] LoggingSettings
    "Logging": {
        "SerializeData": false, // [performance critical] this logs all parameters and results from every traced function. Use with caution, only for debugging.
        "SerializeHttp": false, // [performance critical] this logs all incoming and outgoing http data. Use with caution, only for debugging.
        "ElasticTraceEnabled": true, // enables tracing with ElasticAPM for every traced functions
        "ElasticCacheTraceEnabled": true, // enables cache tracing in applications using Infra.Net.CacheManager
        "ElasticHttpManagerTraceEnabled": true, // enables tracing of specific HttpClientManager actions to projects that uses it
        "ElasticTraceType": "request", // trace type for ElasticAPM
        "LogLevel": {
            "Default": "Debug",
            "Microsoft": "Warning",
            "Microsoft.Hosting.Lifetime": "Information",
            "System.Net.Http.HttpClient": "Warning",
            "Elastic.Apm": "Warning"
        }
    },
    //[...] ElasticSettings
    "ElasticApm": {
        "Enabled": true, // Enables ElasticAPM
        "ServiceName": "ServiceName",
        "LogLevel": "Debug",
        "Environment": "Debug",
        "SecretToken": "token",
        "ServerUrls": "http://localhost:8200",
        "TransactionSampleRate": 1.0,
        "SpanFramesMinDuration": "200ms" 
    },
```

These configs, should give you log entries like these:

``` cs
[...]
2021-02-01 22:26:20.362804 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Executing method "IMongoReadOnlyService`1::Get"[]
2021-02-01 22:26:20.363550 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Executing method "IMongoReadOnlyRepository`1::Get"[]
2021-02-01 22:26:20.364291 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Method executed "IMongoReadOnlyRepository`1::Get"""
2021-02-01 22:26:20.370778 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Method executed "IMongoReadOnlyService`1::Get"""
2021-02-01 22:26:20.432610 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Method executed "IMongoReadOnlyRepository`1::Get"""
2021-02-01 22:26:20.437442 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Method executed "IMongoReadOnlyService`1::Get"""
2021-02-01 22:26:20.464643 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Method executed "IA12nService::HasAuthorizationForAction"""
2021-02-01 22:26:20.804012 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Executing method "IFileUploadService::Upload"[]
2021-02-01 22:26:20.830115 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 HttpCacheManager::"Headers":"Accept:text/plain, Authorization:Basic ..., maxIdleTimeSeconds:1800, timeToLiveSeconds:3600, performAsync:True"
2021-02-01 22:26:20.882228 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 HttpClientManager::"MediaType: text/plain"
2021-02-01 22:26:20.883554 -03:00 [DBG] 80286f64-c5ff-4a5d-8dae-90c366f3b903 HttpClientManager::"PreparingHttpClient: http://localhost:8080/"
2021-02-01 22:26:20.883728 -03:00 [INF] 80286f64-c5ff-4a5d-8dae-90c366f3b903 HttpClientManager::"Send: PUT, http://localhost:8080/rest/default/21665a9c54fc2135afbc6650cd3c23a2d9b118de86911ffa7a7a748654823146"
2021-02-01 22:26:21.049695 -03:00 [INF] 80286f64-c5ff-4a5d-8dae-90c366f3b903 HttpClientManager::"StatusCode: OK 165ms"
2021-02-01 22:26:22.229453 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Executing method "IMongoReadOnlyService`1::Exists"[]
2021-02-01 22:26:22.232885 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Executing method "IMongoReadOnlyRepository`1::Get"[]
2021-02-01 22:26:22.253131 -03:00 [VRB] 80286f64-c5ff-4a5d-8dae-90c366f3b903 Executing method "IMongoService`1::Create"[]
[...]
```

More on that soon.


## Infra.Net.Extensions

This project has some cool extensions that might be useful in some cases. E.g.:

``` cs
DateTime.LastDayOfMonth();
DateTime.LastDayOfMonth();
Dictionary<string, string>.BuildResource(); //HttpContext - builds URL resources
string.ExtractNumbers();
string.ToPascalCase();
string.ToNormalForm(); //securely removes all special characters from a string: façade => facade; joão => joao

```

## Infra.Net.DataAccess.MongoDb

MongoDB driver for .NET with some abstractions. Some cool stuff can be made using `Infra.Net.DataAccess.MongoDb.WebHelpers` as well. 

Docs soon.

## Infra.Net.DomainExtensions

Some useful extensions for countrywide contexts (mainly for BR - more to come)

## Infra.Net.Helpers

HttpClient helpers


## Infra.Net.SwaggerFilters

Swagger filters for general use. Optimized for Swagger v5+

## How to use with ElasticAPM

Comming soon

## How to use with Polly

Comming soon

## How to use with ElasticAPM

Comming soon