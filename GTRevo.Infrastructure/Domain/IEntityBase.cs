using System;

namespace GTRevo.Infrastructure.Domain
{
    public interface IEntityBase : IComponent
    {
        Guid Id { get; }
    }
}
