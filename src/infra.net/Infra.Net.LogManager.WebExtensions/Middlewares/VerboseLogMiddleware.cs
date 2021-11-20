using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Infra.Net.Extensions.Extensions;

namespace Infra.Net.LogManager.WebExtensions.Middlewares;

public class VerboseLogMiddleware : LogMiddlewareBase
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    private readonly List<string> _filterUrl;
    //TODO move to default configuration
    private string DefaultFilterUrl = "/swagger,/favicon,/apiMonitor";

    public VerboseLogMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _configuration = configuration;

        _filterUrl = DefaultFilterUrl.Split(',').Select(x => x).ToList();

        var filters = configuration["Logging:FilterUrls"];
        if (!string.IsNullOrWhiteSpace(filters))
            foreach (var s in filters.Split(','))
                _filterUrl.Add(s.Trim());
    }

    public async Task Invoke(HttpContext context, ILogger logger)
    {
#if DEBUG
        //HotReload only for Debugging
        if (!logger.IsEnabled(LogEventLevel.Verbose) || 
            !bool.Parse(_configuration["Logging:SerializeHttp"] ?? "false"))
        {
            await _next(context);
            return;
        }
#endif
        var stopwatch = new Stopwatch();
        _log = logger;
        await using var responseBody = new MemoryStream();
        var originalBodyStream = context.Response.Body;
        try
        {
            Log(LogEventLevel.Verbose, await FormatRequest(context));

            stopwatch.Start();
            context.Response.Body = responseBody;
            await _next(context);

            if (!IsStatusCodeRedirect(context.Response.StatusCode))
                Log(LogEventLevel.Verbose, await FormatResponse(context, stopwatch));
        }
        catch (Exception ex)
        {
            if (!IsStatusCodeRedirect(context.Response.StatusCode))
                Log(LogEventLevel.Error, await FormatResponse(context, stopwatch), ex);
            throw;
        }
        finally
        {
            if (!IsStatusCodeRedirect(context.Response.StatusCode))
                await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private bool ShouldLog(HttpContext context)
    {
        if (_filterUrl == null)
            return true;

        return !_filterUrl.Any(x => context.Request.Path.ToString().ContainsIgnoreCase(x));
    }

    private async Task<string> FormatRequest(HttpContext context)
    {
        var request = context.Request;
        var bodyAsText = await request.ExtractRequestBody();

        if (!ShouldLog(context))
            return null;
        var r = $"Request {request.Scheme} {request.Method} {request.Host}{request.PathBase}{request.Path}";
        if (_log.IsEnabled(LogEventLevel.Verbose))
            r += $" {request.ContentType} QueryString=\"{request.QueryString}\" " +
                 $"Body=\"{bodyAsText}\"";
        return r;
    }

    private async Task<string> FormatResponse(HttpContext context, Stopwatch stopwatch = null)
    {
        var request = context.Request;
        var response = context.Response;
        var text = string.Empty;
        if (response.Body.CanSeek)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
        }

        if (!ShouldLog(context))
            return null;
        var r = $"Response {response.StatusCode} {request.Method} {request.Host}{request.PathBase}{request.Path} {response.ContentType} {stopwatch?.ElapsedMilliseconds}ms";
        if (_log.IsEnabled(LogEventLevel.Verbose))
            r += $" Body=\"{text}\"";
        return r;
    }

    private bool IsStatusCodeRedirect(int statusCode)
    {
        switch (statusCode)
        {
            case { } x
                when x < 200 || (x >= 300 && x < 400): //https://github.com/aliostad/CacheCow/issues/241
                return true;
        }

        return false;
    }
}