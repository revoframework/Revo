using System;

namespace GTRevo.Infrastructure.Core.Domain.Basic
{
    public abstract class BasicClassEntity : BasicEntity, IHasClassId<Guid>
    {
        public BasicClassEntity(Guid id)
            : base(id)
        {
        }

        protected BasicClassEntity()
        { 
        }

        /// <summary>
        /// Just for the convenience when storing entities in RDBMS.
        /// Should be automatically injected by the repository on the first save/load.
        /// </summary>
        public Guid ClassId { get; private set; }
    }
}
