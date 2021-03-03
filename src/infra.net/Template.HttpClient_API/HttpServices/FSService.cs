using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Infra.Net.HttpClientManager;

namespace Template.HttpClient_API.HttpServices
{
    public interface IFSService
    {
        Task<dynamic> GetFiles();
    }
    public class FSService : IFSService
    {
        // _clientManager isn't exposed publicly
        private readonly IHttpClientManager _clientManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;

        public FSService(IHttpClientManager clientManagerManager, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _clientManager = clientManagerManager;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }

        public async Task<dynamic> GetFiles()
        {
            return await _clientManager.GetAsync<dynamic>($"Search/List", baseUrl: _configuration["FakeApi2"],
                headers: new Dictionary<string, string>()
                {
                    {"Authorization", _contextAccessor.HttpContext.Request.Headers["Authorization"]}
                });
        }
    }
}