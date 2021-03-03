using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Infra.Net.Helpers
{
    public static class ProxyHelper
    {
        public static int GetClientTimeout(IConfiguration config)
        {
            var clientTimeout = int.Parse(config["HttpClientTimeout"] ?? "2");
            return clientTimeout < 1 ? 1 : clientTimeout > 20 ? 20 : clientTimeout;
        }

        public static HttpClientHandler CreateHttpClientHandler(IConfiguration config, string defaultBaseUrlConfig)
        {
            var proxyEnabled = bool.Parse(config["Proxy:Enabled"] ?? "false");
            var baseUrl = string.IsNullOrEmpty(defaultBaseUrlConfig) ? null : config[defaultBaseUrlConfig];

            return proxyEnabled && !ShouldBypassProxy(baseUrl, config) ?
                    CreateSimpleHttpClientHandler(config) :
                    CreateProxiedHttpClientHandler(config);
        }

        private static HttpClientHandler CreateSimpleHttpClientHandler(IConfiguration config)
        {
            var allowAutoRedirect = bool.Parse(config["HttpManager:AllowAutoRedirect"] ?? "false");
            var useCookies = bool.Parse(config["HttpManager:UseCookies"] ?? "false");
            var ignoreCertificateValidation =
                bool.Parse(config["HttpManager:IgnoreCertificateValidation"] ?? "false");

            var handler = new HttpClientHandler { AllowAutoRedirect = allowAutoRedirect, UseCookies = useCookies };
            if (ignoreCertificateValidation)
            {
                handler.CheckCertificateRevocationList = false;
                handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
            }
            SetNetworkCredentials(config, handler);

            return handler;
        }
        private static HttpClientHandler CreateProxiedHttpClientHandler(IConfiguration config)
        {
            var allowAutoRedirect = bool.Parse(config["Proxy:AllowAutoRedirect"] ?? "false");
            var useCookies = bool.Parse(config["Proxy:UseCookies"] ?? "false");
            var proxyUrl = config["Proxy:Address"];
            var bypassLocal = bool.Parse(config["Proxy:BypassLocal"] ?? "false");
            var ignoreCertificateValidation =
                bool.Parse(config["Proxy:IgnoreCertificateValidation"] ?? "false");


            var handler = new HttpClientHandler { AllowAutoRedirect = allowAutoRedirect, UseCookies = useCookies };
            if (!string.IsNullOrWhiteSpace(proxyUrl))
            {
                var bypassList = GetProxyBypassList(config).ToArray();

                handler.Proxy = new WebProxy(proxyUrl, bypassLocal, bypassList.All(string.IsNullOrWhiteSpace) ? null : bypassList);
                handler.UseProxy = true;

                var proxyCredentialsEnabled = bool.Parse(config["Proxy:ProxyCredentials:Enabled"] ?? "false");
                if (proxyCredentialsEnabled)
                {
                    var user = config["Proxy:ProxyCredentials:User"];
                    var password = config["Proxy:ProxyCredentials:Password"];
                    var domain = config["Proxy:ProxyCredentials:Domain"];
                    handler.Proxy.Credentials = new NetworkCredential(user, password, domain);
                }
            }

            if (ignoreCertificateValidation)
            {
                handler.CheckCertificateRevocationList = false;
                handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
            }
            SetNetworkCredentials(config, handler);

            return handler;
        }

        private static void SetNetworkCredentials(IConfiguration config, HttpClientHandler handler)
        {
            var networkCredentialsEnabled = bool.Parse(config["Network:Credentials:Enabled"] ?? "false");
            if (networkCredentialsEnabled)
            {
                var user = config["Network:Credentials:User"];
                var password = config["Network:Credentials:Password"];
                var domain = config["Network:Credentials:Domain"];
                handler.Credentials = new NetworkCredential(user, password, domain);
            }
        }
        public static bool IsProxyException(this Uri destinationUri, IConfiguration config)
            => ShouldBypassProxy(destinationUri.Host, config);

        public static bool ShouldBypassProxy(string baseUrl, IConfiguration config)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return true;
            
            var exceptionList = GetProxyBypassList(config);

            foreach (var exp in exceptionList)
                if (Regex.IsMatch(baseUrl, exp.Trim(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    return true;

            return false;
        }

        private static IEnumerable<string> GetProxyBypassList(IConfiguration config)
        {
            var exceptions = config["Proxy:Exceptions"] ?? string.Empty;
            return exceptions.Split(',');
        }

    }
}
