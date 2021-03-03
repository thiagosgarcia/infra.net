using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Polly;

namespace Infra.Net.HttpClientManager
{
    public interface IHttpClientManager
    {
        string DefaultBaseUrl { get; }
        JsonSerializerSettings ApplicationJsonProps { get; }
        Task<HttpResponseMessage> DeleteAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
        CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> OptionsAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> HeadAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> TraceAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PostFileAsync(
            string resource, IEnumerable<IFormFile> files,
            IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null,
            string baseUrl = null,
            IEnumerable<HttpContent> additionalContent = null,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PostMultipartAsync(string resource, IEnumerable<HttpContent> multipartContent,
            IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> GetAsync(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null);

        Task<Stream> GetStreamAsync(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);
        Task<string> GetStringAsync(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);

        Task<T> GetAsync<T>(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class;

        Task<HttpResponseMessage> PostAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null);

        Task<Stream> PostStreamAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);
        Task<string> PostStringAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);

        Task<T> PostAsync<T>(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class;

        Task<HttpResponseMessage> PutAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null);

        Task<Stream> PutStreamAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);
        Task<string> PutStringAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);

        Task<T> PutAsync<T>(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class;

        Task<HttpResponseMessage> PatchAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null);

        Task<Stream> PatchStreamAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);
        Task<string> PatchStringAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null);

        Task<T> PatchAsync<T>(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class;
    }
}