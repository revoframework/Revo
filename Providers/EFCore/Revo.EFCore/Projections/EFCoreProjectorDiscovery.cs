using Microsoft.Extensions.Logging;
using Ninject;
using Revo.Core.Types;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreProjectorDiscovery : ProjectorDiscovery
    {
        public EFCoreProjectorDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel, ILogger logger)
            : base(typeExplorer, kernel, new [] { typeof(IEFCoreEntityEventProjector<>), typeof(IEFCoreSyncEntityEventProjector<>) }, logger)
        {
        }
    }
}
