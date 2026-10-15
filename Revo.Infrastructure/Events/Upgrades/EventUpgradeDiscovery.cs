using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Revo.Core.Configuration;
using Revo.Core.Events;
using Revo.Core.Types;

namespace Revo.Infrastructure.Events.Upgrades;

public class EventUpgradeDiscovery(IRevoConfiguration configuration, ILogger<EventUpgradeDiscovery> logger)
{
    public void DiscoverEventUpgrades(IServiceCollection serviceCollection)
    {
        var typeExplorer = new TypeExplorer(
            configuration.GetSection<CoreConfigurationSection>(),
            NullLogger<TypeExplorer>.Instance);

        var upgradeTypes = typeExplorer.GetAllTypes()
            .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition
                        && typeof(IEventUpgrade).IsAssignableFrom(x))
            .ToArray();

        RegisterUpgrades(serviceCollection, upgradeTypes);
        logger.LogDebug($"Discovered {upgradeTypes.Length} event upgrades: {string.Join(", ", upgradeTypes.Select(x => x.FullName))}");
    }

    private static void RegisterUpgrades(IServiceCollection serviceCollection, IEnumerable<Type> upgradeTypes)
    {
        foreach (Type upgradeType in upgradeTypes)
        {
            serviceCollection.AddTransient(typeof(IEventUpgrade), upgradeType);
        }
    }
}
