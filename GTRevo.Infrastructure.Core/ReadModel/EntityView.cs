using System;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.Core.ReadModel
{
    [DatabaseEntity(SchemaSpace = "ReadModel")]
    public abstract class EntityView : IHasId<Guid>
    {
        public Guid Id { get; set; }
    }
}
