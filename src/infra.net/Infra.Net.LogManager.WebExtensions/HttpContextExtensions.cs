using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Infra.Net.Extensions.ExceptionHandlers;
using Infra.Net.Extensions.Extensions;
using Infra.Net.LogManager.WebExtensions.Middlewares;

namespace Infra.Net.LogManager.WebExtensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Extrai o Body do HttpRequest e reaplica o ponteiro correto para a property, 
        /// de modo que possa ser utilizado em outras etapas da pipeline
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string ExtractBody(this HttpRequest request)
            => InternalExtractBody(request);

        /// <summary>
        /// Extrai o Body do HttpRequest e reaplica o ponteiro correto para a property, 
        /// de modo que possa ser utilizado em outras etapas da pipeline
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string ExtractBody(this HttpResponse response)
            => InternalExtractBody(response);

        private static string InternalExtractBody(dynamic request)
        {
            var bodyAsText = new StreamReader(request.Body).ReadToEnd();
            var bodyData = Encoding.UTF8.GetBytes(bodyAsText);
            request.Body = new MemoryStream(bodyData);
            return bodyAsText;
        }
        public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder app)
        {
#if DEBUG
            //HotReload only for Debugging
            return app
                .UseMiddleware<VerboseLogMiddleware>()
                .UseMiddleware<LogMiddleware>();
#else
            var logger = (ILogger)app.ApplicationServices.GetService(typeof(ILogger));
            var config = (IConfiguration)app.ApplicationServices.GetService(typeof(IConfiguration));
            if ((logger?.IsEnabled(LogEventLevel.Verbose) ?? false) &&
                bool.Parse(config?["Logging:SerializeHttp"] ?? "false"))
                return app.UseMiddleware<VerboseLogMiddleware>();

            return app.UseMiddleware<LogMiddleware>();
#endif
        }

        public static async Task<string> ExtractRequestBody(this HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            RewindBody(body);

            var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            RewindBody(body);
            request.Body = body;
            return bodyAsText;
        }

        private static void RewindBody(Stream body)
        {
            if (body.CanSeek)
                body.Seek(0, SeekOrigin.Begin);
        }

        public static bool HasAuthHeader(this HttpContext context)
        {
            var headers = context?.Request?.Headers;
            if (headers == null || !headers.ContainsKey("Authorization"))
                return false;

            var authHeader = Regex.Replace(headers["Authorization"].ToString(), "(?<=^)(Bearer)", "", RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrWhiteSpace(authHeader))
                return false;

            return true;
        }

        public static AuthHeader ExtractInfoFromAuthHeader(this HttpContext context, bool validateToken = false)
        {
            var headers = context?.Request?.Headers;
            if (headers == null || !headers.ContainsKey("Authorization"))
                return null;

            var authHeader = Regex.Replace(headers["Authorization"].ToString(), "(?<=^)(Bearer)", "", RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrWhiteSpace(authHeader))
                return null;

            try
            {
                var token = new JwtSecurityToken(jwtEncodedString: authHeader);
                var nbf = token.Claims.FirstOrDefault(c => c.Type == "nbf")?.Value;
                var exp = token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (validateToken)
                    ValidateTokenDates(nbf, exp);

                var json = token.Claims.FirstOrDefault(c => c.Type == "objeto")?.Value;
                dynamic djson = JsonConvert.DeserializeObject(json);

                var profile = new List<string>();
                if (djson?.perfilLogin?.Type.ToString() == "String")
                    profile.Add(djson?.perfilLogin.ToString());
                else
                    profile = djson?.perfilLogin?.ToObject<List<string>>();

                return new AuthHeader
                {
                    login = djson?.login?.Value,
                    guid = djson?.guid?.Value,
                    cpfCnpj = djson?.cpF_CNPJ?.Value,
                    profiles = profile?.ToArray() ?? Array.Empty<string>()
                };
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static void ValidateTokenDates(string nbf, string exp)
        {
            var notBefore = DateTime.Parse(nbf.Trim('"'));
            if (notBefore.AddMinutes(-5).IsAfter(DateTime.Now)) //5-minute tolerance
                throw new UnauthorizedException();

            var expire = DateTime.Parse((exp.Trim('"')));
            if (expire.AddMinutes(5).IsBefore(DateTime.Now)) //5-minute tolerance
                throw new UnauthorizedException();
        }

        public class AuthHeader
        {
            public string login { get; set; }
            public string guid { get; set; }
            public string cpfCnpj { get; set; }
            public string[] profiles { get; set; }
        }
    }
}
