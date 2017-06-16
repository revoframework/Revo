using System;
using GTRevo.DataAccess.Entities;

namespace GTRevo.Infrastructure.Domain.Basic
{
    /// <summary>
    /// Aggregate root that is typically persisted to a RDBMS store using an ORM.
    /// </summary>
    [DatabaseEntity]
    public abstract class BasicAggregateRoot : AggregateRoot, IQueryableEntity
    {
        public BasicAggregateRoot(Guid id) : base(id)
        {
        }

        public BasicAggregateRoot()
        {
        }
    }
}
