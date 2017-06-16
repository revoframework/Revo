using System;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.ReadModel
{
    [DatabaseEntity(SchemaSpace = "ReadModel")]
    public abstract class EntityView : IHasId<Guid>
    {
        public Guid Id { get; set; }
    }
}
