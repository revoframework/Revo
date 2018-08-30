using System;
using Revo.Core.Configuration;
using Revo.EF6.DataAccess;
using Revo.EF6.Events;
using Revo.EF6.EventStores;
using Revo.EF6.Projections;
using Revo.EF6.Sagas;
using Revo.Infrastructure;

namespace Revo.EF6
{
    public static class EF6InfrastructureConfigurationExtensions
    {
        public static IRevoConfiguration UseAllEF6Infrastructure(this IRevoConfiguration configuration,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            return configuration
                .UseEF6DataAccess(null)
                .UseEF6AsyncEvents()
                .UseEF6EventStore()
                .UseEF6Projections()
                .UseEF6Sagas();
        }

        public static IRevoConfiguration UseEF6EventStore(this IRevoConfiguration configuration,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EF6InfrastructureConfigurationSection>();
            section.UseEventStore = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseEventStore)
                {
                    c.LoadModule<EF6EventStoreModule>();
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEF6Sagas(this IRevoConfiguration configuration,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EF6InfrastructureConfigurationSection>();
            section.UseSagas = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseSagas)
                {
                    c.LoadModule<EF6SagasModule>();
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEF6Projections(this IRevoConfiguration configuration,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EF6InfrastructureConfigurationSection>();
            section.UseProjections = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseProjections)
                {
                    c.LoadModule<EF6ProjectionsModule>();
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEF6AsyncEvents(this IRevoConfiguration configuration,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureInfrastructure();

            var section = configuration.GetSection<EF6InfrastructureConfigurationSection>();
            section.UseAsyncEvents = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseAsyncEvents)
                {
                    c.LoadModule<EF6AsyncEventsModule>();
                }
            });

            return configuration;
        }
    }
}
