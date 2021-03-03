using System;
using System.Reflection;

namespace Infra.Net.LogManager
{
    public class DispatchExceptionArgs : EventArgs
    {
        public DispatchExceptionArgs(MethodInfo methodInfo, Exception exception)
        {
            MethodInfo = methodInfo;
            Exception = exception;
        }

        public MethodInfo MethodInfo { get; set; }
        public Exception Exception { get; set; }
    }

    public class DispatchAfterExecutionArgs : EventArgs
    {
        public DispatchAfterExecutionArgs(MethodInfo methodInfo, object returnValue)
        {
            MethodInfo = methodInfo;
            ReturnValue = returnValue;
        }

        public MethodInfo MethodInfo { get; set; }
        public object ReturnValue { get; set; }
    }

    public class DispatchBeforeExecutionArgs : EventArgs
    {
        public MethodInfo MethodInfo { get; }
        public object[] Args { get; }

        public DispatchBeforeExecutionArgs(MethodInfo methodInfo, object[] args)
        {
            MethodInfo = methodInfo;
            Args = args;
        }
    }
}