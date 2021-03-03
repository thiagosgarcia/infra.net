using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Template.HttpClient_API.HttpServices;
using Infra.Net.LogManager.WebExtensions.Attributes;

namespace Template.HttpClient_API.Controllers
{
    [HandleException]
    [ApiController]
    [Route("[controller]")]
    public class EntityController : ControllerBase
    {
        private readonly EntityService _entityService;

        public EntityController(EntityService entityService)
        {
            _entityService = entityService;
        }

        /// <summary>
        /// Exemplo de erro
        /// </summary>
        /// <returns></returns>
        [HttpGet("ModelEntity")]
        public Task<IEnumerable<ModelEntity>> Get()
        {
            return _entityService.GetEntities();
        }

        /// <summary>
        /// Exemplo lentidão para tratar com Polly
        /// </summary>
        /// <returns></returns>
        [HttpGet("ModelEntity/Lento")]
        public Task<IEnumerable<ModelEntity>> Slow(int waitTime = 10)
        {
            Thread.Sleep(TimeSpan.FromSeconds(waitTime));
            return _entityService.GetEntities2();
        }

        [HttpGet("Entity2")]
        public Task<IEnumerable<ModelEntity>> Get(string filter, string orderBy)
        {
            var @params = new Dictionary<string, string>()
            {
                {nameof(filter), filter},
                {nameof(orderBy), orderBy}
            };
            var headers = new Dictionary<string, string>()
            {
                {nameof(filter), filter},
                {nameof(orderBy), orderBy}
            };
            return _entityService.GetEntitiesByFilter(@params, @headers);
        }
    }
}
