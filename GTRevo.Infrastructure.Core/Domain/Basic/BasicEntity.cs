using System;
using GTRevo.DataAccess.Entities;

namespace GTRevo.Infrastructure.Domain.Basic
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
