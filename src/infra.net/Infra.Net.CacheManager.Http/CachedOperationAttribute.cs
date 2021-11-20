using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Infra.Net.CacheManager.Http;

[AttributeUsage(AttributeTargets.Method)]
public class CachedOperationAttribute : TypeFilterAttribute
{
    public CachedOperationAttribute(string cacheId = null, int expireTimeMinutes = 5, int idleTimeMinutes = 0) : base(typeof(CachedOperationImpl))
    {
        Arguments = new object[] { cacheId ?? "default", expireTimeMinutes, idleTimeMinutes };
    }

    protected class CachedOperationImpl : IAsyncActionFilter
    {
        private readonly ICacheManager _cacheManager;
        private readonly string _cacheId;
        private readonly int _expireTimeMinutes;
        private readonly int _idleTimeMinutes;

        public CachedOperationImpl(ICacheManager cacheManager, string cacheId, int expireTimeMinutes, int idleTimeMinutes)
        {
            _cacheManager = cacheManager;
            _cacheId = cacheId;
            _expireTimeMinutes = expireTimeMinutes;
            _idleTimeMinutes = idleTimeMinutes;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var @params = context.ActionArguments.Select(x => $"{x.Key}_{x.Value}");
            var key = $"CachedOperation_{context.RouteData.Values["action"]}_{context.RouteData.Values["controller"]}";
                
            var expireTime = _expireTimeMinutes > 0 ? TimeSpan.FromMinutes(_expireTimeMinutes) : TimeSpan.FromMinutes(5);
            var idleTime = _expireTimeMinutes > 0 ? TimeSpan.FromMinutes(_idleTimeMinutes) : (TimeSpan?) null;

            context.Result = await _cacheManager.GetOrPut<ObjectResult>(_cacheId, key,
                async () =>
                {
                    var response = await next();
                    return (ObjectResult)response.Result;
                }, @params, expireTime, idleTime, performAsync: true);
        }
    }
}