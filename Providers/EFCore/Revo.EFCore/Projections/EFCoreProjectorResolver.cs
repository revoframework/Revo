using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreProjectorResolver : IEFCoreProjectorResolver
    {
        private readonly IKernel kernel;

        public EFCoreProjectorResolver(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public bool HasAnyProjectors(Type aggregateType)
        {
            var bindings = kernel.GetBindings(
                typeof(IEFCoreEntityEventProjector<>).MakeGenericType(aggregateType));
            return bindings.Any();
        }

        public bool HasAnySyncProjectors(Type aggregateType)
        {
            var bindings = kernel.GetBindings(
                typeof(IEFCoreSyncEntityEventProjector<>).MakeGenericType(aggregateType));
            return bindings.Any();
        }

        public IReadOnlyCollection<IEntityEventProjector> GetProjectors(Type aggregateType)
        {
            return kernel.GetAll(
                    typeof(IEFCoreEntityEventProjector<>).MakeGenericType(aggregateType))
                .Cast<IEntityEventProjector>()
                .ToArray();
        }

        public IReadOnlyCollection<IEntityEventProjector> GetSyncProjectors(Type aggregateType)
        {
            return kernel.GetAll(
                    typeof(IEFCoreSyncEntityEventProjector<>).MakeGenericType(aggregateType))
                .Cast<IEntityEventProjector>()
                .ToArray();
        }
    }
}
