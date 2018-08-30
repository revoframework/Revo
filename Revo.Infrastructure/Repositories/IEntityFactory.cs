using System;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Repositories
{
    public interface IEntityFactory
    {
        IEntity ConstructEntity(Type entityType, Guid id/*, params object[] parameters*/);
    }
}
