using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.ReadModel
{
    [DatabaseEntity]
    public abstract class EntityView : IHasId<Guid>
    {
        public Guid Id { get; set; }
    }
}
