using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Polly.CircuitBreaker;
using Serilog;
using Infra.Net.Extensions.ExceptionHandlers;
using Newtonsoft.Json;

namespace Infra.Net.LogManager.WebExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HandleExceptionAttribute : TypeFilterAttribute
    {
        public HandleExceptionAttribute() : base(typeof(HandleExceptionAttributeImpl))
        {
        }
        public HandleExceptionAttribute(Type t) : base(t)
        {
        }

        public class HandleExceptionAttributeImpl : ExceptionFilterAttribute
        {
            private readonly IHttpContextAccessor _contextAccessor;
            private readonly ILogger _logger;

            public HandleExceptionAttributeImpl(IHttpContextAccessor contextAccessor, ILogger logger)
            {
                _contextAccessor = contextAccessor;
                _logger = logger;
            }
            /// <inheritdoc />

            public override void OnException(ExceptionContext context)
            {
                context.Result = GenerateErrorResult(context);
            }

            private ObjectResult GenerateErrorResult(ExceptionContext context)
            {
                if (context.Exception is AuthorizationException authorizationException)
                    return GenerateAuthExceptionResult(authorizationException);

                if (context.Exception is FaultException faultException)
                    return FaultExceptionResult(faultException);

                if (context.Exception is ArgumentStringInvalidException argumentStringInvalidException)
                    return GenerateArgumentStringExceptionResult(argumentStringInvalidException);

                if (context.Exception is BrokenCircuitException brokenCircuitException)
                    return GenerateBrokenCircuitExceptionResult(brokenCircuitException);

                return GenerateExceptionResult(ExtractStatusCode(context.Exception), context.Exception);
            }

            private int ExtractStatusCode(Exception contextException)
            {
                if (contextException is UnauthorizedException)
                    return (int)HttpStatusCode.Unauthorized;

                if (contextException is AuthorizationException)
                    return (int)HttpStatusCode.Forbidden;

                if (contextException is AggregateException)
                    return (int)HttpStatusCode.InternalServerError;

                if (contextException is HttpRequestException ||
                    contextException is FaultException ||
                    contextException is ArgumentOutOfRangeException ||
                    contextException is ArgumentStringInvalidException)
                    return (int)HttpStatusCode.NotFound;

                if (contextException is BrokenCircuitException)
                    return (int)HttpStatusCode.TooManyRequests;

                if (contextException is ValidationException)
                    return (int)HttpStatusCode.NotAcceptable;

                return (int)HttpStatusCode.NotFound;
            }

            private ObjectResult FaultExceptionResult(Exception contextException)
            {
                return GenerateExceptionResult((int)HttpStatusCode.NotAcceptable, ((dynamic)contextException).Detail ?? contextException);
            }

            protected ObjectResult GenerateExceptionResult(int statusCode, object value)
            {
                var result = new ObjectResult(null)
                {
                    StatusCode = statusCode,
                    Value = value
                };
                return GenerateExceptionResult(result);
            }
            protected ObjectResult GenerateAuthExceptionResult(AuthorizationException exception)
            {
                var result = new ObjectResult(null)
                {
                    StatusCode = ExtractStatusCode(exception),
                    Value = new
                    {
                        exception.Login,
                        exception.RequestPath,
#if DEBUG
                        exception.StringArguments,
#endif
                        exception.Status,
                        CorrelationId = _contextAccessor.GetCorrelationId(),
                        DateTime = DateTime.Now
                    }
                };
                return GenerateExceptionResult(result);
            }
            protected ObjectResult GenerateArgumentStringExceptionResult(ArgumentStringInvalidException ex)
            {
                var result = new ObjectResult(null)
                {
                    StatusCode = ExtractStatusCode(ex),
                    Value = new
                    {
                        Mensagem = $"O valor da variável {ex.Name} não pode ser {ex.Value}",
                        Variavel = ex.Name,
                        Valor = ex.Value,
                        CorrelationId = _contextAccessor.GetCorrelationId(),
                        DateTime = DateTime.Now
                    }
                };
                return GenerateExceptionResult(result);
            }
            private ObjectResult GenerateBrokenCircuitExceptionResult(BrokenCircuitException ex)
            {
                var result = new ObjectResult(null)
                {
                    StatusCode = ExtractStatusCode(ex),
                    Value = new
                    {
                        Mensagem = "Muitas requisições para o mesmo endpoint falharam. O circuito agora está aberto.",
                        RetryInMinutes = 1,
                    }
                };
                return GenerateExceptionResult(result);
            }
            
            protected ObjectResult GenerateExceptionResult(ObjectResult result)
            {
                var prefix = "HandleException::";
                if (result.Value is Exception exception)
                    _logger.Error(exception, prefix);
                else
                    _logger.Error($"{prefix}\n{{0}}", result.Value);

                result.Value = new
                {
#if DEBUG
                    SerializedException = JsonConvert.SerializeObject(result.Value),
#endif
                    Message = result.Value is Exception ex ? ex.Message : null,
                    Value = result.Value is Exception ? null : result.Value,
                    CorrelationId = _contextAccessor.GetCorrelationId(),
                    DateTime = DateTime.Now
                };

                return result;
            }
        }
    }
}
