using System;
using Revo.Core.Configuration;
using Revo.EF6.DataAccess;
using Revo.EF6.Events;
using Revo.EF6.EventStores;
using Revo.EF6.Projections;
using Revo.EF6.Repositories;
using Revo.EF6.Sagas;
using Revo.Infrastructure;

namespace Revo.EF6.Configuration
{
    public static class EF6InfrastructureConfigurationExtensions
    {
        public static IRevoConfiguration UseAllEF6Infrastructure(this IRevoConfiguration configuration,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            configuration
                .UseEF6DataAccess(null)
                .UseEF6Repositories()
                .UseEF6AsyncEvents()
                .UseEF6EventStore()
                .UseEF6Projections()
                .UseEF6Sagas();
            
            var section = configuration.GetSection<EF6InfrastructureConfigurationSection>();
            advancedAction?.Invoke(section);

            return configuration;
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

        public static IRevoConfiguration UseEF6Repositories(this IRevoConfiguration configuration,
            bool useCrudAggregateStore = true, bool useEventSourcedAggregateStore = true,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EF6InfrastructureConfigurationSection>();
            section.UseCrudAggregateStore = useCrudAggregateStore;
            section.UseEventSourcedAggregateStore = useEventSourcedAggregateStore;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                var dataAccessSection = configuration.GetSection<EF6DataAccessConfigurationSection>();
                if (!dataAccessSection.IsActive || !dataAccessSection.UseAsPrimaryRepository)
                {
                    throw new InvalidOperationException("EF6 data access must be enabled and configured as primary data repository in order to use EF6 aggregate stores.");

                    // TODO ensure EF6 aggregate stores get injected the correct EF6 repository/event store (in case there are more registered)
                    // to make it possible to use it even when not configured as primary data access
                }

                c.LoadModule(new EF6RepositoriesModule(section));
            });

            return configuration;
        }

        public static IRevoConfiguration UseEF6Projections(this IRevoConfiguration configuration,
            bool autoDiscoverProjectors = true,
            Action<EF6InfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EF6InfrastructureConfigurationSection>();
            section.UseProjections = true;
            section.AutoDiscoverProjectors = autoDiscoverProjectors;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseProjections)
                {
                    c.LoadModule(new EF6ProjectionsModule(section));
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
                    c.LoadModule(new EF6AsyncEventsModule(section.CustomizeEventJsonSerializer));
                }
            });

            return configuration;
        }
    }
}
