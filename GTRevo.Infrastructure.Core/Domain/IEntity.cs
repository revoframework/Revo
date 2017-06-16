using System;

namespace GTRevo.Infrastructure.Domain
{
    public interface IEntity : IComponent, IHasId<Guid>
    {
    }
}
