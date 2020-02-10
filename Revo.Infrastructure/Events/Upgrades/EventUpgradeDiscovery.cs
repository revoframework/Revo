using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Infrastructure.Events.Upgrades
{
    public class EventUpgradeDiscovery : IApplicationConfigurer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public EventUpgradeDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
        }

        public void Configure()
        {
            Discover();
        }

        private void Discover()
        {
            var upgradeTypes = typeExplorer.GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition
                    && typeof(IEventUpgrade).IsAssignableFrom(x))
                .ToArray();

            RegisterUpgrades(upgradeTypes);
            Logger.Debug($"Discovered {upgradeTypes.Length} event upgrades: {string.Join(", ", upgradeTypes.Select(x => x.FullName))}");
        }

        private void RegisterUpgrades(IEnumerable<Type> upgradeTypes)
        {
            foreach (Type upgradeType in upgradeTypes)
            {
                kernel
                    .Bind<IEventUpgrade>()
                    .To(upgradeType)
                    .InTransientScope();
            }
        }
    }
}