using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Infra.Net.LogManager.WebExtensions
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CorrelationIdEnricher(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "CorrelationId", _contextAccessor.GetCorrelationId()));
        }
    }
}