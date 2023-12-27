
namespace Infra.Net.Extensions.Extensions;

public static class HttpExtensions
{
    public static string BuildResource(this IDictionary<string, string> @params)
    {
        var query = string.Empty;

        if (@params != null && @params.Any())
            query += "?" + string.Join("&", @params.Where(x => x.Value != null)
                .Select(x =>
                {
                    var key = Regex.Replace(x.Key.Trim(), @"(\[\d+\]$)", "");
                    return $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(x.Value)}";
                }));

        return query;
    }

    public static string BuildResource(this IDictionary<string, string> @params, string basePath)
        => basePath + @params.BuildResource();
    public static string BuildResource(this string basePath, IDictionary<string, string> @params)
        => basePath + @params.BuildResource();

    public static void Add(this HttpRequestHeaders requestHeaders, IDictionary<string, string> headers)
    {
        if (headers != null && headers.Any())
            foreach (var header in headers)
                requestHeaders.Add(header.Key, header.Value);
    }
    public static void ForceAdd(this HttpRequestHeaders requestHeaders, IDictionary<string, string> headers)
    {
        if (headers != null && headers.Any())
            foreach (var header in headers)
                requestHeaders.TryAddWithoutValidation(header.Key, header.Value);
    }
}
