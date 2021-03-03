using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Infra.Net.DataAccess.MongoDb.Repository;
using Infra.Net.DataAccess.MongoDb.WebHelpers;
using Infra.Net.LogManager.WebExtensions.Attributes;

namespace Template.HttpClient_API.Controllers
{

    public class TestRepository : MongoRepository<TestEntity>
    {
        public TestRepository(IConfiguration config)
            : base("ApiMonitorDb", "TestDb", "TestCollection", config)
        {
        }
    }
    public class TestEntity : MongoEntity
    {
        public DateTime ATime { get; set; }
        public string AString { get; set; }
        public bool ABool { get; set; }
    }

    [HandleException]
    [ApiController]
    [Route("api/[controller]")]
    public class MongoController : MongoApiController<TestEntity>
    {
        public MongoController(IMongoHttpService<TestEntity> service) : base(service)
        {
        }

        [HttpGet("Exists/{guid}")]
        public Task<bool> Exists(string guid) => Service.Exists(e => e.Id == guid);

    }
}
