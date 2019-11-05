using System;
using Revo.Core.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.Events;
using Revo.EFCore.EventStores;
using Revo.EFCore.Projections;
using Revo.EFCore.Sagas;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.EFCore.Configuration
{
    public static class EFCoreInfrastructureConfigurationExtensions
    {
        public static IRevoConfiguration UseAllEFCoreInfrastructure(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            return configuration
                .UseEFCoreDataAccess(null)
                .UseEFCoreRepositories()
                .UseEFCoreAsyncEvents()
                .UseEFCoreEventStore()
                .UseEFCoreProjections()
                .UseEFCoreSagas();
        }

        public static IRevoConfiguration UseEFCoreEventStore(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureInfrastructure(config =>
            {
                config.DatabaseMigrations.AddScannedAssembly(
                    new ResourceDatabaseMigrationDiscoveryAssembly(
                        typeof(InfrastructureConfigurationSection).Assembly.FullName,
                        "Sql"));
            });

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

        public static IRevoConfiguration UseEFCoreRepositories(this IRevoConfiguration configuration,
            bool useCrudAggregateStore = true, bool useEventSourcedAggregateStore = true,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EFCoreInfrastructureConfigurationSection>();
            section.UseCrudAggregateStore = useCrudAggregateStore;
            section.UseEventSourcedAggregateStore = useEventSourcedAggregateStore;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                c.LoadModule(new EFCoreRepositoriesModule(section));
            });

            return configuration;
        }

        public static IRevoConfiguration UseEFCoreSagas(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureInfrastructure(config =>
            {
                config.DatabaseMigrations.AddScannedAssembly(
                    new ResourceDatabaseMigrationDiscoveryAssembly(
                        typeof(InfrastructureConfigurationSection).Assembly.FullName,
                        "Sql"));
            });

            var section = configuration.GetSection<EFCoreInfrastructureConfigurationSection>();
            section.UseSagas = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseSagas)
                {
                    c.LoadModule<EFCoreSagasModule>();
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEFCoreProjections(this IRevoConfiguration configuration,
            bool autoDiscoverProjectors = true,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EFCoreInfrastructureConfigurationSection>();
            section.UseProjections = true;
            section.AutoDiscoverProjectors = autoDiscoverProjectors;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseProjections)
                {
                    c.LoadModule(new EFCoreProjectionsModule(section));
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseEFCoreAsyncEvents(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureInfrastructure(config =>
            {
                config.DatabaseMigrations.AddScannedAssembly(
                    new ResourceDatabaseMigrationDiscoveryAssembly(
                        typeof(InfrastructureConfigurationSection).Assembly.FullName,
                        "Sql"));
            });

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
