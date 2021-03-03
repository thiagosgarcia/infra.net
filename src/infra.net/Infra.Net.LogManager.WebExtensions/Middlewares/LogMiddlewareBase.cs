using System;
using Serilog;
using Serilog.Events;

namespace Infra.Net.LogManager.WebExtensions.Middlewares
{
    public class LogMiddlewareBase
    {
        protected ILogger _log;
        protected void Log(LogEventLevel level, string msg, Exception ex = null)
        {
            if (!(_log?.IsEnabled(level) ?? false) || string.IsNullOrEmpty(msg))
                return;

            _log.Write(level, ex, "LogMiddleware::{0}", msg);
        }
    }
}