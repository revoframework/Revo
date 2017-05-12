using System;
using GTRevo.DataAccess.EF6;

namespace GTRevo.Infrastructure.Domain.Basic
{
    /// <summary>
    /// Aggregate root that is typically persisted to a RDBMS store using an ORM.
    /// </summary>
    [GTRevo.DataAccess.EF6.DatabaseEntity]
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
