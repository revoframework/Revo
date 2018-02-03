using System;
using Revo.DataAccess.Entities;
using Revo.Domain.Core;

namespace Revo.Domain.ReadModel
{
    public abstract class EntityReadModel : ReadModelBase, IHasId<Guid>, IManuallyRowVersioned
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
