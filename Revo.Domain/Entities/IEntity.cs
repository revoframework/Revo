using System;
using Revo.DataAccess.Entities;
using Revo.Domain.Core;

namespace Revo.Domain.Entities
{
    /// <summary>
    /// An entity with an ID.
    /// </summary>
    public interface IEntity : IComponent, IHasId<Guid>
    {
    }
}
