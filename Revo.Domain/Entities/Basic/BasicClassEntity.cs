using System;
using Revo.Domain.Core;

namespace Revo.Domain.Entities.Basic
{
    public abstract class BasicClassEntity : BasicEntity, IBasicClassIdEntity, IHasClassId<Guid>
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

        Guid IBasicClassIdEntity.ClassId
        {
            get => ClassId;
            set => ClassId = value;
        }
    }
}
