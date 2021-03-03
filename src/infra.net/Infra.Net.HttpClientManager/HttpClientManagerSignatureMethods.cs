using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;

namespace Infra.Net.HttpClientManager
{
    public partial class HttpClientManager
    {
        private Task<HttpResponseMessage> InternalGet(string resource,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> @params, bool throwOnError,
            CancellationToken cancellationToken,
            string mediaType, MediaTransformation mediaTransform, IAsyncPolicy policyWrap)
                => PerformAction(HttpMethod.Get, resource, null, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);

        public Task<HttpResponseMessage> GetAsync(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null)
            => InternalGet(resource, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);

        public async Task<Stream> GetStreamAsync(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await GetAsync(resource, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStreamAsync();
        }
        public async Task<string> GetStringAsync(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await GetAsync(resource, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStringAsync();
        }
        public async Task<T> GetAsync<T>(string resource, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class
        {
            string result = await GetStringAsync(resource, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, policyWrap);
            return JsonConvert.DeserializeObject<T>(result, ApplicationJsonProps ?? DefaultJsonProps);
        }


        private Task<HttpResponseMessage> InternalPost(string resource, dynamic obj,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> @params, bool throwOnError,
            CancellationToken cancellationToken,
            string mediaType, MediaTransformation mediaTransform, IAsyncPolicy policyWrap = null)
            => PerformAction(HttpMethod.Post, resource, obj, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);
        public Task<HttpResponseMessage> PostAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null)
            => InternalPost(resource, obj, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);

        public async Task<Stream> PostStreamAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await PostAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStreamAsync();
        }
        public async Task<string> PostStringAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await PostAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStringAsync();
        }
        public async Task<T> PostAsync<T>(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class
        {
            string result = await PostStringAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, policyWrap);
            return JsonConvert.DeserializeObject<T>(result, ApplicationJsonProps ?? DefaultJsonProps);
        }


        private Task<HttpResponseMessage> InternalPut(string resource, dynamic obj,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> @params, bool throwOnError,
            CancellationToken cancellationToken,
            string mediaType, MediaTransformation mediaTransform, IAsyncPolicy policyWrap = null)
            => PerformAction(HttpMethod.Put, resource, obj, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);
        public Task<HttpResponseMessage> PutAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null)
            => InternalPut(resource, obj, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);

        public async Task<Stream> PutStreamAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await PutAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStreamAsync();
        }
        public async Task<string> PutStringAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await PutAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStringAsync();
        }
        public async Task<T> PutAsync<T>(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class
        {
            string result = await PutStringAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, policyWrap);
            return JsonConvert.DeserializeObject<T>(result, ApplicationJsonProps ?? DefaultJsonProps);
        }


        private Task<HttpResponseMessage> InternalPatch(string resource, dynamic obj,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> @params, bool throwOnError,
            CancellationToken cancellationToken,
            string mediaType, MediaTransformation mediaTransform, IAsyncPolicy policyWrap = null)
            => PerformAction(HttpMethod.Patch, resource, obj, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);
        public Task<HttpResponseMessage> PatchAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, bool throwOnError = false, IAsyncPolicy policyWrap = null)
            => InternalPatch(resource, obj, headers, baseUrl, @params, throwOnError, cancellationToken, mediaType, mediaTransform, policyWrap);

        public async Task<Stream> PatchStreamAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await PatchAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStreamAsync();
        }
        public async Task<string> PatchStringAsync(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null)
        {
            HttpResponseMessage result = await PatchAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, true, policyWrap);
            return await result.Content.ReadAsStringAsync();
        }
        public async Task<T> PatchAsync<T>(string resource, dynamic obj, IDictionary<string, string> @params = null,
            IDictionary<string, string> headers = null, string baseUrl = null,
            CancellationToken cancellationToken = default, string mediaType = null,
            MediaTransformation mediaTransform = MediaTransformation.SerializeJson, IAsyncPolicy policyWrap = null) where T : class
        {
            string result = await PatchStringAsync(resource, obj, @params, headers, baseUrl, cancellationToken, mediaType, mediaTransform, policyWrap);
            return JsonConvert.DeserializeObject<T>(result, ApplicationJsonProps ?? DefaultJsonProps);
        }
    }
}
