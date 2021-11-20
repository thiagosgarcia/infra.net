
namespace Infra.Net.Extensions.Extensions;

public static class PagedResultExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> list, IRequestParameters<T> request) where T : class
    {
        return PagedResult<T>.Create(list, request);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this Task<IQueryable<T>> list, IRequestParameters<T> request) where T : class
    {
        return await PagedResult<T>.CreateAsync(list, request);
    }
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> list, IRequestParameters<T> request) where T : class
    {
        return PagedResult<T>.Create(list, request);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this Task<IEnumerable<T>> list, IRequestParameters<T> request) where T : class
    {
        return await PagedResult<T>.CreateAsync(list, request);
    }
}