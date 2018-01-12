using System;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.Core.ReadModel
{
    public abstract class EntityReadModel : ReadModelBase, IHasId<Guid>, IManuallyRowVersioned
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
