using System;
using System.Linq.Expressions;
using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.ReadModel
{
    public abstract class EntityReadModel : ReadModelBase, IHasId<Guid>, IRowVersioned
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
