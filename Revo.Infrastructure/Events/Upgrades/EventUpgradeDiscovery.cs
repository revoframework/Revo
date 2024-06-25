﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ninject;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Infrastructure.Events.Upgrades
{
    public class EventUpgradeDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel,
        ILogger logger) : IApplicationConfigurer
    {
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
            logger.LogDebug($"Discovered {upgradeTypes.Length} event upgrades: {string.Join(", ", upgradeTypes.Select(x => x.FullName))}");
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