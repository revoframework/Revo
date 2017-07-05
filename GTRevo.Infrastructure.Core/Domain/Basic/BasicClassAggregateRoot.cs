using System;

namespace GTRevo.Infrastructure.Core.Domain.Basic
{
    public abstract class BasicClassAggregateRoot : BasicAggregateRoot, IHasClassId<Guid>
    {
        public BasicClassAggregateRoot(Guid id) : base(id)
        {
        }

        public BasicClassAggregateRoot()
        {
        }

        /// <summary>
        /// Just for the convenience when storing entities in RDBMS.
        /// Should be automatically injected by the repository on the first save/load.
        /// </summary>
        public virtual Guid ClassId { get; private set; }
    }
}
