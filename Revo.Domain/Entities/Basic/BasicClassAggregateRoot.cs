using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.Entities.Basic
{
    public abstract class BasicClassAggregateRoot : BasicAggregateRoot, IBasicClassIdEntity, IHasClassId<Guid>
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

        Guid IBasicClassIdEntity.ClassId
        {
            get => ClassId;
            set => ClassId = value;
        }
    }
}
