using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreProjectorResolver : IEFCoreProjectorResolver
    {
        private readonly IServiceProvider serviceProvider;

        public EFCoreProjectorResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool HasAnyProjectors(Type aggregateType)
        {
            return serviceProvider.GetServices(
                    typeof(IEFCoreEntityEventProjector<>).MakeGenericType(aggregateType))
                .Any();
        }

        public bool HasAnySyncProjectors(Type aggregateType)
        {
            return serviceProvider.GetServices(
                    typeof(IEFCoreSyncEntityEventProjector<>).MakeGenericType(aggregateType))
                .Any();
        }

        public IReadOnlyCollection<IEntityEventProjector> GetProjectors(Type aggregateType)
        {
            return serviceProvider.GetServices(
                    typeof(IEFCoreEntityEventProjector<>).MakeGenericType(aggregateType))
                .Cast<IEntityEventProjector>()
                .ToArray();
        }

        public IReadOnlyCollection<IEntityEventProjector> GetSyncProjectors(Type aggregateType)
        {
            return serviceProvider.GetServices(
                    typeof(IEFCoreSyncEntityEventProjector<>).MakeGenericType(aggregateType))
                .Cast<IEntityEventProjector>()
                .ToArray();
        }
    }
}
