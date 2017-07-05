using System;

namespace GTRevo.Infrastructure.Core.Domain
{
    public interface IEntity : IComponent, IHasId<Guid>
    {
    }
}
