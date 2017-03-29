using System;

namespace GTRevo.Infrastructure.Domain
{
    public interface IEntityBase : IComponent, IHasId<Guid>
    {
    }
}
