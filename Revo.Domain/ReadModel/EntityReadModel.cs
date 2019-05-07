using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.ReadModel
{
    public abstract class EntityReadModel : ReadModelBase, IEntityReadModel, IManuallyRowVersioned
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
