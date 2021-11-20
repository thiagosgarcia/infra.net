using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Infra.Net.LogManager.WebExtensions;

public static class WebLogHelper
{
    private static readonly object _locker = new();

    public static Guid GetCorrelationId(this IHttpContextAccessor contextAccessor)
    {
        StringValues headerValue = new();
        if (contextAccessor.HttpContext?.Request.Headers.TryGetValue("CorrelationId", out headerValue) ?? false)
            if (Guid.TryParse(headerValue, out var parsedGuid))
                return parsedGuid;

        lock (_locker)
        {
            var headerCorrId = Guid.NewGuid();
            SetCorrelationIdHeaders(contextAccessor, headerCorrId);
            return headerCorrId;
        }
    }

    private static void SetCorrelationIdHeaders(IHttpContextAccessor contextAccessor, Guid guid)
    {
        contextAccessor.HttpContext?.Request.Headers.TryAdd("CorrelationId", guid.ToString());
    }
}