using Ninject;
using Revo.Core.Types;
using Revo.Infrastructure.Projections;

namespace Revo.RavenDB.Projections
{
    public class RavenProjectorDiscovery : ProjectorDiscovery
    {
        public RavenProjectorDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
            : base(typeExplorer, kernel, new [] { typeof(IRavenEntityEventProjector<>) })
        {
        }
    }
}
