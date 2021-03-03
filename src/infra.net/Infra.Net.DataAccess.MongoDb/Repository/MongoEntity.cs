using System;

namespace Infra.Net.DataAccess.MongoDb.Repository
{
    public class MongoEntity
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
