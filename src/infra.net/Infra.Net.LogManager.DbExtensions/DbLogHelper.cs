using System.Text.RegularExpressions;
using Serilog;
using Serilog.Events;

namespace Infra.Net.LogManager.DbExtensions
{
    public static class DbLogHelper
    {
        public static void LogDbContextTransaction(ILogger logger, string dbLog, string contextName)
        {
            var level = Regex.IsMatch(dbLog, "^(-- :?p)|(SELECT)") ? LogEventLevel.Verbose : LogEventLevel.Debug;
            logger.Write(level, $"SQL {contextName} {dbLog.Trim()}");
        }
    }
}
