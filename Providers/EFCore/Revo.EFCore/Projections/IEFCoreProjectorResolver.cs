using System;
using System.Collections.Generic;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public interface IEFCoreProjectorResolver
    {
        IReadOnlyCollection<IEntityEventProjector> GetProjectors(Type aggregateType);
        IReadOnlyCollection<IEntityEventProjector> GetSyncProjectors(Type aggregateType);
        bool HasAnyProjectors(Type aggregateType);
        bool HasAnySyncProjectors(Type aggregateType);
    }
}