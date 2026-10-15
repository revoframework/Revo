using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Revo.Core.Configuration;
using Revo.Core.Types;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections;

public static class EFCoreProjectorDiscovery
{
    public static void DiscoverProjectors(IRevoConfiguration configuration, IServiceCollection services)
    {
        var typeExplorer = new TypeExplorer(
            configuration.GetSection<CoreConfigurationSection>(),
            NullLogger<TypeExplorer>.Instance);

        var discovery = new ProjectorDiscovery(
            typeExplorer,
            new[] { typeof(IEFCoreEntityEventProjector<>), typeof(IEFCoreSyncEntityEventProjector<>) },
            NullLogger.Instance);

        discovery.DiscoverProjectors(services);
    }
}
