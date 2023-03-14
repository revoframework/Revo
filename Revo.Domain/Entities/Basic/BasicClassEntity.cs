using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.Entities.Basic
{
    /// <summary>
    /// Entity that is typically persisted to a RDBMS store using an ORM.
    /// For convenience, it also automatically stores its ClassId as one of its properties.
    /// </summary>
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
