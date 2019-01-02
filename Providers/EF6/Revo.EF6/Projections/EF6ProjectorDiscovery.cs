using Ninject;
using Revo.Core.Types;
using Revo.Infrastructure.Projections;

namespace Revo.EF6.Projections
{
    public class EF6ProjectorDiscovery : ProjectorDiscovery
    {
        public EF6ProjectorDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
            : base(typeExplorer, kernel, new [] { typeof(IEF6EntityEventProjector<>) })
        {
        }
    }
}
