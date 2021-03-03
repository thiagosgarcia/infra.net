using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Infra.Net.DataAccess.MongoDb.Repository;

namespace Infra.Net.DataAccess.MongoDb.WebHelpers
{
    public abstract class MongoApiController<T> : MongoReadOnlyApiController<T>
        where T : MongoEntity
    {
        public MongoApiController(IMongoHttpService<T> service)
            : base(service)
        {
        }

        /// <summary>
        /// HttpPost para iniciar ações de inserção da entidade no repositório
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        /// <returns>Entidade adicionada no banco</returns>
        [HttpPost("")]
        public virtual Task<T> Post(T entity)
        {
            return Service.Create(entity);
        }

        /// <summary>
        /// HttpPut/Patch para iniciar ações de atualização da entidade no repositório
        /// </summary>
        /// <param name="guid">Identificador de entidade a ser atualizada</param>
        /// <param name="entity">Nova entidade</param>
        /// <returns>Entidade atualizada no banco</returns>
        [HttpPut("{guid}")]
        public virtual Task<T> Put(string guid, T entity)
        {
            return Service.Update(guid, entity);
        }

        /// <summary>
        /// HttpDelete para iniciar ações de exclusão da entidade no repositório representada pelo seu identificador primário
        /// </summary>
        /// <param name="entity">Identificador primário da entidade a ser excluída</param>
        /// <returns>Número de itens excluídos</returns>
        [HttpDelete("{guid}")]
        public virtual Task<long> Delete(string guid)
        {
            return Service.Remove(guid);
        }

        /// <summary>
        /// HttpDelete para iniciar ações de exclusão da entidade no repositório representada pelo seu identificador primário
        /// </summary>
        /// <param name="guid">Lista de GUIDs de identificadores para exclusão</param>
        /// <returns>Número de itens excluídos</returns>
        [HttpDelete("")]
        public virtual Task<long> Delete([FromBody]List<string> guid)
        {
            return Service.Remove(guid);
        }

    }
}