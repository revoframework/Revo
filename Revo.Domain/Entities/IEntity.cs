using System;
using Revo.Domain.Core;

namespace Revo.Domain.Entities
{
    public interface IEntity : IComponent, IHasId<Guid>
    {
    }
}
