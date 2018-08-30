using System;
using Revo.Core.Configuration;
using Revo.EFCore.Events;
using Revo.EFCore.EventStores;
using Revo.EFCore.Projections;
using Revo.Infrastructure;

namespace Revo.EFCore.Configuration
{
    public static class EFCoreInfrastructureConfigurationExtensions
    {
        public static IRevoConfiguration UseAllEFCoreInfrastructure(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            return configuration
                .UseEFCoreAsyncEvents()
                .UseEFCoreEventStore()
                .UseEFCoreProjections()
                .UseEFCoreSagas();
        }

        public static IRevoConfiguration UseEFCoreEventStore(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EFCoreInfrastructureConfigurationSection>();
            section.UseEventStore = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseEventStore)
                {
                    c.LoadModule<EFCoreEventStoreModule>();
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEFCoreSagas(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EFCoreInfrastructureConfigurationSection>();
            section.UseSagas = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseSagas)
                {
                    //c.LoadModule<EFCoreSagasModule>();
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEFCoreProjections(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EFCoreInfrastructureConfigurationSection>();
            section.UseProjections = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseProjections)
                {
                    c.LoadModule<EFCoreProjectionsModule>();
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEFCoreAsyncEvents(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureInfrastructure();

            var section = configuration.GetSection<EFCoreInfrastructureConfigurationSection>();
            section.UseAsyncEvents = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseAsyncEvents)
                {
                    c.LoadModule<EFCoreAsyncEventsModule>();
                }
            });

            return configuration;
        }
    }
}
