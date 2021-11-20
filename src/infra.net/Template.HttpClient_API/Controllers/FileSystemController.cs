using Microsoft.AspNetCore.Mvc;
using Template.HttpClient_API.HttpServices;
using Infra.Net.HttpClientManager;
using Infra.Net.LogManager.WebExtensions.Attributes;

namespace Template.HttpClient_API.Controllers;

[HandleException]
[ApiController]
[Route("[controller]")]
public class FileSystemController : ControllerBase
{
    private readonly IHttpClientManager _clientManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IFSService _fsService;

    public FileSystemController(IHttpClientManager clientManager, IConfiguration configuration, IHttpContextAccessor contextAccessor, IFSService fsService)
    {
        _clientManager = clientManager;
        _configuration = configuration;
        _contextAccessor = contextAccessor;
        _fsService = fsService;
    }

    [HttpGet("{id}/Download")]
    public Task<HttpResponseMessage> Download(string id)
        => _clientManager.GetAsync($"File/{id}/Download", baseUrl: _configuration["FakeApi2"]);


    [HttpPost]
    public Task<HttpResponseMessage> Upload(IFormFile file, string storage, bool? blockPermalink = null)
        => _clientManager.PostAsync($"File", file, baseUrl: _configuration["FakeApi2"]);

    [HttpDelete("{id}")]
    public Task<HttpResponseMessage> Delete(string id)
        => _clientManager.DeleteAsync($"File/{id}", baseUrl: _configuration["FakeApi2"]);

    [HttpGet("List")]
    public Task<dynamic> List(int page = 0, int perPage = 100)
        => _fsService.GetFiles();
}