using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Infra.Net.Extensions.Extensions;

namespace Infra.Net.LogManager.WebExtensions.Middlewares
{
    public class LogMiddleware : LogMiddlewareBase
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        private readonly List<string> _filterUrl;
        //TODO move to default configuration
        private string DefaultFilterUrl = "/swagger,/favicon,/apiMonitor";

        public LogMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _configuration = configuration;

            _filterUrl = DefaultFilterUrl.Split(',').Select(x => x).ToList();
            var filters = configuration["Logging:FilterUrls"];
            if (!string.IsNullOrWhiteSpace(filters))
            {
                foreach (var s in filters.Split(','))
                {
                    _filterUrl.Add(s.Trim());
                }
            }
        }

        public async Task Invoke(HttpContext context, ILogger logger)
        {
#if DEBUG
            //HotReload only for Debugging
            if (logger.IsEnabled(LogEventLevel.Verbose) &&
                bool.Parse(_configuration["Logging:SerializeHttp"] ?? "false"))
            {
                await _next(context);
                return;
            }
#endif
            var stopwatch = new Stopwatch();
            _log = logger;
            try
            {
                Log(LogEventLevel.Information, FormatRequest(context));
                stopwatch.Start();
                await _next(context);
                Log(LogEventLevel.Information, FormatResponse(context, stopwatch));
            }
            catch (Exception ex)
            {
                Log(LogEventLevel.Error, FormatResponse(context, stopwatch), ex);
                throw;
            }
        }

        private bool ShouldLog(HttpContext context)
        {
            if (_filterUrl == null)
                return true;

            return !_filterUrl.Any(x => context.Request.Path.ToString().ContainsIgnoreCase(x));
        }

        private string FormatRequest(HttpContext context)
        {
            if (!ShouldLog(context))
                return null;

            var request = context.Request;
            return $"Request {request.Scheme} {request.Method} {request.Host}{request.PathBase}{request.Path}";
        }

        private string FormatResponse(HttpContext context, Stopwatch stopwatch = null)
        {
            if (!ShouldLog(context))
                return null;

            var request = context.Request;
            var response = context.Response;
            return $"Response {response.StatusCode} {request.Method} {request.Host}{request.PathBase}{request.Path} {response.ContentType} {stopwatch?.ElapsedMilliseconds}ms";
        }
    }
}
