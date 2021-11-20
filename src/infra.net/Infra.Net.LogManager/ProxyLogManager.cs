using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace Infra.Net.LogManager;

public class ProxyLogManager<T> : DispatchProxy
{
    private readonly JsonSerializerSettings _defaultJsonProps = new()
    {
        DateTimeZoneHandling = DateTimeZoneHandling.Local,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    public event EventHandler<DispatchBeforeExecutionArgs> BeforeExecute;
    public event EventHandler<DispatchAfterExecutionArgs> AfterExecute;
    public event EventHandler<DispatchExceptionArgs> ErrorExecuting;

    public T DecoratedInterface { get; set; }
    public ILogger Logger { get; set; }
    public List<string> BlockedNames { get; set; }
    public bool SerializeData { get; set; }

    private Predicate<MethodInfo> _predicateFilter;
    private LogEventLevel DefaultLevel { get; set; } = LogEventLevel.Information;

    public Predicate<MethodInfo> Filter
    {
        get => _predicateFilter;
        set => _predicateFilter = value ?? (m => true);
    }

    [DebuggerStepThrough]
    protected override object Invoke(MethodInfo methodInfo, object[] args)
    {
        try
        {
            OnBeforeExecute(methodInfo, args);
            var result = methodInfo.Invoke(DecoratedInterface, args);
            if (result is Task task)
            {
                task.ContinueWith(x =>
                {
                    if (x.IsFaulted)
                        OnErrorExecuting(methodInfo, x.Exception);
                    if (x.IsCanceled)
                        OnAfterExecute(methodInfo, "Task has been canceled");
                    if (x.IsCompleted)
                        OnAfterExecute(methodInfo, (result as dynamic)?.Result ?? result);
                });
                return task;
            }

            OnAfterExecute(methodInfo, result);
            return result;
        }
        catch (Exception ex)
        {
            var theException = ex.InnerException ?? ex;
            OnErrorExecuting(methodInfo, theException);
            throw theException;
        }
    }

    [DebuggerStepThrough]
    public static T Create(T decoratedInterface, IServiceProvider services, LogEventLevel defaultLogLevel)
    {
        object proxy = Create<T, ProxyLogManager<T>>();
        ((ProxyLogManager<T>)proxy).SetParameters(
            decoratedInterface,
            (ILogger)services.GetService(typeof(ILogger)),
            (IConfiguration)services.GetService(typeof(IConfiguration)),
            defaultLogLevel);

        return (T)proxy;
    }

    [DebuggerStepThrough]
    private void SetParameters(T decoratedInterface, ILogger logger, IConfiguration configuration, LogEventLevel logLevel)
    {
        if (decoratedInterface == null)
            throw new ArgumentNullException(nameof(decoratedInterface));

        DecoratedInterface = decoratedInterface;
        Logger = logger;
        SerializeData = Convert.ToBoolean(configuration["Logging:SerializeData"] ?? "false"); // We can turn off data serialization in appSettings

        SetDefaultLevel(logger, logLevel);
        ConfigureSerializationFilters(configuration);
        ConfigureHandlers();
    }

    [DebuggerStepThrough]
    private void SetDefaultLevel(ILogger logger, LogEventLevel logLevel)
    {
        DefaultLevel =
            logger.IsEnabled(LogEventLevel.Verbose) ?
                LogEventLevel.Verbose :
                logLevel;
    }

    [DebuggerStepThrough]
    private void ConfigureSerializationFilters(IConfiguration configuration)
    {
        var serializationFilterConfig = configuration["Logging:SerializationFilterOverride"];
        BlockedNames ??= new List<string>();

        Filter = m =>
        {
            foreach (var name in BlockedNames)
            {
                if (m?.DeclaringType?.Name?.Contains(name) ?? false)
                    return false;
                if (m?.DeclaringType?.Namespace?.Contains(name) ?? false)
                    return false;
                if (m?.Name?.Contains(name) ?? false)
                    return false;
            }

            return true;
        };

        if (string.IsNullOrWhiteSpace(serializationFilterConfig))
            return;

        foreach (var name in serializationFilterConfig.Split(','))
            if (!BlockedNames.Contains(name.Trim()))
                BlockedNames.Add(name.Trim());
    }

    [DebuggerStepThrough]
    private object FilterValueTypes(object value, string name = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            if (BlockedNames.Any(x => name.ToLower().Contains(x.ToLower())))
                return null;

        //TODO Filter value types that doesn't make sense to serialize
        if (value == null)
            return null;

        var type = value.GetType();
        if (value is System.Type)
            return null;

        if (value is Stream || value is byte[] ||
            type.IsAssignableFrom(typeof(Stream)) ||
            type.IsAssignableFrom(typeof(byte)))
            return null;

        if ((type.IsAssignableFrom(typeof(Task))) ||
            (type.IsGenericType &&
             type.GetGenericTypeDefinition() == typeof(Task<>)))
            return null;

        if (!string.IsNullOrWhiteSpace(name))
            return new { paramName = name, value };

        return new { value };
    }


    // In the future, more handlers can be added and removed via injection or configurations in order to do specific logging logic
    [DebuggerStepThrough]
    public void ConfigureHandlers()
    {
        BeforeExecute += BeforeHandler();
        AfterExecute += AfterHandler();
        ErrorExecuting += ErrorHandler();
    }

    [DebuggerStepThrough]
    private EventHandler<DispatchBeforeExecutionArgs> BeforeHandler() =>
        (sender, beforeExecutionArgs) =>
        {
            var className = beforeExecutionArgs.MethodInfo?.DeclaringType?.Name ?? string.Empty;

            Logger.Write(DefaultLevel, "Executing method {0}{1}",
                $"{className}::{beforeExecutionArgs.MethodInfo?.Name}",
                GetBeforeExecutionArgs(beforeExecutionArgs, Logger)
            );
        };

    [DebuggerStepThrough]
    private EventHandler<DispatchAfterExecutionArgs> AfterHandler() =>
        (sender, afterExecutionArgs) =>
        {
            var className = afterExecutionArgs.MethodInfo?.DeclaringType?.Name ?? string.Empty;

            Logger.Write(DefaultLevel, "Method executed {0}{1}",
                $"{className}::{afterExecutionArgs?.MethodInfo?.Name}",
                GetAfterExecutionReturnValue(afterExecutionArgs, Logger)
            );
        };

    [DebuggerStepThrough]
    private EventHandler<DispatchExceptionArgs> ErrorHandler() =>
        (sender, exceptionArgs) =>
        {
            var className = exceptionArgs.MethodInfo?.DeclaringType?.Name ?? string.Empty;

            Logger.Write(LogEventLevel.Error, exceptionArgs.Exception,
                "Exception caught {0}",
                $"{className}::{exceptionArgs?.MethodInfo?.Name}"
            );
        };
    [DebuggerStepThrough]
    private void OnBeforeExecute(MethodInfo methodInfo, object[] args)
    {
        if (BeforeExecute != null)
            if (_predicateFilter(methodInfo))
                BeforeExecute(this, new DispatchBeforeExecutionArgs(methodInfo, args));
    }
    [DebuggerStepThrough]
    private void OnAfterExecute(MethodInfo methodInfo, object result)
    {
        if (AfterExecute != null)
            if (_predicateFilter(methodInfo))
                AfterExecute(this, new DispatchAfterExecutionArgs(methodInfo, result));
    }
    [DebuggerStepThrough]
    private void OnErrorExecuting(MethodInfo methodInfo, Exception ex)
    {
        if (ErrorExecuting != null)
            if (_predicateFilter(methodInfo))
                ErrorExecuting(this, new DispatchExceptionArgs(methodInfo, ex));
    }

    [DebuggerStepThrough]
    private string GetAfterExecutionReturnValue(DispatchAfterExecutionArgs afterExecutionArgs, ILogger logger)
    {
        if (!logger.IsEnabled(LogEventLevel.Verbose) || !SerializeData)
            return string.Empty;

        try
        {
            var returnValue = afterExecutionArgs.ReturnValue;
            return JsonConvert.SerializeObject(FilterValueTypes(returnValue), _defaultJsonProps);
        }
        catch (Exception)
        {
            return "Unable to serialize parameter";
        }
    }

    [DebuggerStepThrough]
    private IEnumerable<string> GetBeforeExecutionArgs(DispatchBeforeExecutionArgs beforeExecutionArgs, ILogger logger)
    {
        if (!logger.IsEnabled(LogEventLevel.Verbose) || !SerializeData)
            return new List<string>();

        var r = new List<object>();
        if (beforeExecutionArgs.Args.Length == beforeExecutionArgs.MethodInfo.GetParameters().Length)
            for (var i = 0; i < beforeExecutionArgs.Args.Length; i++)
            {
                try
                {
                    r.Add(JsonConvert.SerializeObject(FilterValueTypes(beforeExecutionArgs.Args[i],
                            beforeExecutionArgs.MethodInfo.GetParameters().ElementAt(i).ToString()),
                        _defaultJsonProps));
                }
                catch (Exception)
                {
                    r.Add("Unable to serialize parameter");
                }
            }

        return r.Select(x => x.ToString());
    }
}