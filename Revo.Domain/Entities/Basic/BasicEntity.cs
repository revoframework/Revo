using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.Entities.Basic
{
    /// <summary>
    /// Entity that is typically persisted to a RDBMS store using an ORM.
    /// </summary>
    [DatabaseEntity]
    public abstract class BasicEntity : Entity, IQueryableEntity, IRowVersioned
    {
        public BasicEntity(Guid id) : base(id)
        {
        }

        protected BasicEntity()
        {
        }

        public int Version { get; set; }
    }
}
