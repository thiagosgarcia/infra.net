using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Infra.Net.DataAccess.MongoDb.Repository;

namespace Infra.Net.DataAccess.MongoDb.WebHelpers
{
    public abstract class MongoReadOnlyApiController<T> : ControllerBase
        where T : MongoEntity
    {

        protected readonly IMongoHttpService<T> Service;

        /// <summary>
        /// Construtor principal com injeção do Serviço.
        /// </summary>
        /// <param name="service"></param>
        public MongoReadOnlyApiController(IMongoHttpService<T> service)
        {
            Service = service;
        }

        /// <summary>
        /// HttpGet sem parâmetros de identificação de campos, com valores opcionais de filtro.
        /// Enviará request ao service, buscando todos os itens do banco que representem os filtros e paginação definidas.
        /// </summary>
        /// <param name="request">Parâmetros de filtro de paginação opcionais</param>
        /// <returns>Lista de acordo com a paginação e filtros definidos</returns>
        [HttpGet("")]
        public virtual Task<IEnumerable<T>> GetAll(int page = 0, int perPage = 10)
        {
            return Service.Get(page, perPage);
        }

        /// <summary>
        /// HttpGet com parâmetro Id para buscar a representação do identificador no repositório
        /// </summary>
        /// <param name="guid">Identificador primário da entidade a ser resolvida</param>
        /// <returns>Representação do banco de dados da entidade em questão</returns>
        [HttpGet("{guid}")]
        public virtual Task<T> Get(string guid)
        {
            return Service.Get(guid);
        }
    }
}
