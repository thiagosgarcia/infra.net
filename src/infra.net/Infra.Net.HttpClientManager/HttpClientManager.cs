
namespace Infra.Net.HttpClientManager;
public partial class HttpClientManager : IHttpClientManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IAsyncPolicy _defaultPolicyWrap;
    private readonly IConfiguration _configuration;
#if DEBUG
    private readonly string _baseUrlConfig;
    public string DefaultBaseUrl { get; set; }
#else
        public string DefaultBaseUrl { get; }
#endif

    public JsonSerializerSettings ApplicationJsonProps { get; }

    private string _assemblyVersion = string.Empty;
    private string _apiName = string.Empty;

    private readonly bool SuppressExceptionSerialization;
    public static readonly JsonSerializerSettings DefaultJsonProps = new()
    {
        DateTimeZoneHandling = DateTimeZoneHandling.Local,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    public HttpClientManager(string baseUrlConfig, JsonSerializerSettings jsonProps,
        IHttpClientFactory httpClientFactory, ILogger logger,
        IConfiguration configuration, IHttpContextAccessor contextAccessor,
        string apiName = null, IAsyncPolicy defaultPolicyWrap = null)
    {
#if DEBUG
        _baseUrlConfig = baseUrlConfig;
#endif
        if (!baseUrlConfig.IsNullOrEmpty())
            DefaultBaseUrl = configuration[baseUrlConfig];

        ApplicationJsonProps = jsonProps;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _contextAccessor = contextAccessor;
        _defaultPolicyWrap = defaultPolicyWrap;

        if (apiName != null)
            _apiName = apiName;

        var version = Assembly.Load("Infra.Net.HttpClientManager").GetName().Version;
        if (version is not null)
            _assemblyVersion = version.ToString();

        SuppressExceptionSerialization = bool.Parse(_configuration["AppConfig:SuppressExceptionSerialization"] ?? "false");

    }

    private void Log(LogEventLevel level, string msg, Exception ex = null)
    {
        if (!(_logger?.IsEnabled(level) ?? false))
            return;

        _logger.Write(level, ex, "HttpClientManager::{0}", msg);
    }

    private Task<HttpResponseMessage> PerformAction(
        HttpMethod httpMethod, string resource, dynamic obj,
        IDictionary<string, string> headers, string baseUrl,
        IDictionary<string, string> @params, bool throwOnError,
        CancellationToken cancellationToken,
        string mediaType, MediaTransformation mediaTransform, IAsyncPolicy policyWrap)
    {
        policyWrap ??= _defaultPolicyWrap;

        if (policyWrap != null)
            return policyWrap.ExecuteAsync<HttpResponseMessage>(() => PerformAction(httpMethod, resource, obj, headers,
                baseUrl, @params,
                throwOnError, cancellationToken, mediaType, mediaTransform));

        return PerformAction(httpMethod, resource, obj, headers, baseUrl, @params,
            throwOnError, cancellationToken, mediaType, mediaTransform);
    }

    private async Task<HttpResponseMessage> PerformAction(
        HttpMethod httpMethod, string resource, dynamic obj,
        IDictionary<string, string> headers, string baseUrl,
        IDictionary<string, string> @params, bool throwOnError,
        CancellationToken cancellationToken,
        string mediaType, MediaTransformation mediaTransform)
    {
        var createContentTask = CreateRequestContent(obj, mediaType, httpMethod, mediaTransform);
#if DEBUG
        if (_baseUrlConfig != null)
            DefaultBaseUrl = _configuration[_baseUrlConfig];
#endif
        var baseAddress = baseUrl ?? DefaultBaseUrl;

        var requestMessage = new HttpRequestMessage(
            httpMethod,
            new Uri(
                new Uri(baseAddress, UriKind.Absolute),
                new Uri(@params.BuildResource(resource), UriKind.Relative)
                ));

        HandleHeaders(headers, requestMessage);

        requestMessage.Content = await createContentTask;
        try
        {
            var result = await SendAsync(baseAddress, requestMessage, cancellationToken);
            await HandleNonSuccess(throwOnError, result);
            return result;
        }
        catch (HttpRequestException ex)
        {
            Log(LogEventLevel.Error, $"Exception:", ex);
            throw;
        }
        finally
        {
            _contextAccessor?.HttpContext?.Response.Headers.TryAdd("x-hcm-v", _assemblyVersion);
        }
    }

    private void HandleHeaders(IDictionary<string, string> headers, HttpRequestMessage requestMessage)
    {
        requestMessage.Headers.Add(headers);

        if (!requestMessage.Headers.Contains("user-agent"))
            requestMessage.Headers.TryAddWithoutValidation("user-agent", $"{_apiName}_hcm-v_{_assemblyVersion}");
    }

    private Task<HttpContent> CreateRequestContent(dynamic obj, string mediaType, HttpMethod httpMethod, MediaTransformation mediaTransform)
    {
        if (httpMethod == HttpMethod.Get)
            return Task.FromResult<HttpContent>(null);

        return Task.Run(() =>
        {
            mediaType ??= "application/json";

            HttpContent requestContent;
            switch (mediaType)
            {
                case "application/x-www-form-urlencoded":
                    requestContent = new FormUrlEncodedContent(obj);
                    break;

                case "text/plain":
                    requestContent = new StringContent(obj, Encoding.UTF8, mediaType);
                    break;

                case { } when mediaTransform == MediaTransformation.None:
                    requestContent = new StringContent(obj, Encoding.UTF8, mediaType);
                    break;

                case { } when mediaTransform == MediaTransformation.UrlEncoded:
                    //TODO tests
                    var dictionary = (IDictionary<string, string>)obj;
                    requestContent = new StringContent(dictionary.BuildResource().TrimStart('?'), Encoding.UTF8, mediaType);
                    break;

                case { } when mediaTransform == MediaTransformation.SerializeJson:
                default:
                    var jsonParam = JsonConvert.SerializeObject(obj, ApplicationJsonProps ?? DefaultJsonProps);
                    requestContent = new StringContent(jsonParam, Encoding.UTF8, mediaType);
                    break;
            }

            Log(LogEventLevel.Verbose, $"MediaType: {mediaType}");
            return requestContent;
        });
    }

    private async Task<HttpResponseMessage> SendAsync(string baseAddress, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        Log(LogEventLevel.Debug, $"PreparingHttpClient: {baseAddress}");
        cancellationToken.ThrowIfCancellationRequested();

        var client = _httpClientFactory.CreateClient(baseAddress);

        Log(LogEventLevel.Information, $"Send: {requestMessage.Method}, {requestMessage.RequestUri}");
        Stopwatch stopwatch = new();
        stopwatch.Start();
        var result = await client.SendAsync(requestMessage, cancellationToken);

        Log(LogEventLevel.Information, $"StatusCode: {result.StatusCode} {stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    private async Task HandleNonSuccess(bool throwOnError, HttpResponseMessage result)
    {
        if (!result.IsSuccessStatusCode && throwOnError)
        {
            if (SuppressExceptionSerialization)
                throw new HttpRequestException(result.ReasonPhrase);

            var content = await result.Content.ReadAsStringAsync();
            Log(LogEventLevel.Debug, $"ResponseContent: {content}");
            Exception obj;
            try
            {
                obj = JsonConvert.DeserializeObject<HttpRequestException>(content, ApplicationJsonProps ?? DefaultJsonProps);
            }
            catch (Exception e)
            {
                throw new HttpRequestException($"StatusCode='{result.StatusCode}' Body='{content}'", e);
            }
            throw new HttpRequestException(obj?.Message ?? result.ReasonPhrase, obj);
        }
    }

    public Task<HttpResponseMessage> DeleteAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var baseAddress = baseUrl ?? DefaultBaseUrl;
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, baseAddress + @params.BuildResource(resource));
        requestMessage.Headers.Add(headers);
        return SendAsync(baseAddress, requestMessage, cancellationToken);
    }

    public Task<HttpResponseMessage> OptionsAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var baseAddress = baseUrl ?? DefaultBaseUrl;
        var requestMessage = new HttpRequestMessage(HttpMethod.Options, baseAddress + @params.BuildResource(resource));
        requestMessage.Headers.Add(headers);
        return SendAsync(baseAddress, requestMessage, cancellationToken);
    }
    public Task<HttpResponseMessage> HeadAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var baseAddress = baseUrl ?? DefaultBaseUrl;
        var requestMessage = new HttpRequestMessage(HttpMethod.Head, baseAddress + @params.BuildResource(resource));
        requestMessage.Headers.Add(headers);
        return SendAsync(baseAddress, requestMessage, cancellationToken);
    }
    public Task<HttpResponseMessage> TraceAsync(string resource, IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var baseAddress = baseUrl ?? DefaultBaseUrl;
        var requestMessage = new HttpRequestMessage(HttpMethod.Trace, baseAddress + @params.BuildResource(resource));
        requestMessage.Headers.Add(headers);
        return SendAsync(baseAddress, requestMessage, cancellationToken);
    }

    public Task<HttpResponseMessage> PostFileAsync(
        string resource, IEnumerable<IFormFile> files,
        IDictionary<string, string> @params = null,
        IDictionary<string, string> headers = null,
        string baseUrl = null,
        IEnumerable<HttpContent> additionalContent = null,
        CancellationToken cancellationToken = default)
    {
        var baseAddress = baseUrl ?? DefaultBaseUrl;

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, baseAddress + @params.BuildResource(resource));

        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

        var content = new MultipartFormDataContent($"----------{Guid.NewGuid()}");

        if (files != null)
            foreach (var formFile in files)
                content.Add(new StreamContent(formFile.OpenReadStream()), formFile.Name, formFile.FileName);

        if (additionalContent != null)
            foreach (var addContent in additionalContent)
                content.Add(addContent);

        requestMessage.Content = content;
        requestMessage.Headers.Add(headers);

        return SendAsync(baseAddress, requestMessage, cancellationToken);
    }
    public Task<HttpResponseMessage> PostMultipartAsync(string resource, IEnumerable<HttpContent> multipartContent,
        IDictionary<string, string> @params = null, IDictionary<string, string> headers = null, string baseUrl = null,
        CancellationToken cancellationToken = default)
    {
        return PostFileAsync(resource, null, @params, headers, baseUrl, multipartContent, cancellationToken);
    }
}
