using System;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.EventSourcing
{
    public interface IEventSourcedEntityFactory
    {
        IEntity ConstructEntity(Type entityType, Guid id, params object[] parameters);
    }
}
