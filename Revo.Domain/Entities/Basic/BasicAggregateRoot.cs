using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.Entities.Basic
{
    /// <summary>
    /// Basic aggregate root type that is typically persisted using its visible state data
    /// (as compared to other types persisting only their events, etc.).
    /// This is usually desirable when working with an ORM and RDBMS or document database.
    /// </summary>
    [DatabaseEntity]
    public abstract class BasicAggregateRoot : AggregateRoot, IQueryableEntity, IRowVersioned
    {
        public BasicAggregateRoot(Guid id) : base(id)
        {
        }

        protected BasicAggregateRoot()
        {
        }

        public new int Version
        {
            get { return base.Version; }
            set { base.Version = value;  }
        }

        protected sealed override void Publish<T>(T evt)
        {
            base.Publish(evt);
        }
    }
}
