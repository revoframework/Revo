using System;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.ReadModel
{
    public abstract class EntityReadModel : ReadModelBase, IHasId<Guid>, IRowVersioned
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
