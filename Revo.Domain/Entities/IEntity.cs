using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.Entities
{
    /// <summary>
    /// An entity with an ID.
    /// </summary>
    public interface IEntity : IComponent, IHasId<Guid>
    {
    }
}
