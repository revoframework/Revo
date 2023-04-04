using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Revo.Core.Configuration;
using Revo.Core.Transactions;
using Revo.Core.Types;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.Events;
using Revo.EFCore.EventStores;
using Revo.EFCore.Projections;
using Revo.EFCore.Repositories;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;
using Revo.Infrastructure.Events.Configuration;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;
using Revo.Infrastructure.Repositories;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Sagas.Generic;

namespace Revo.EFCore.Configuration
{
    public static class EFCoreInfrastructureConfigurationExtensions
    {
        /// <summary>
        /// Registers EF Core infrastructure (repositories, async events, event store, projections, sagas).
        /// When <paramref name="useDefaults"/> is true, all subparts are enabled; set to false to opt in explicitly via the callback.
        /// </summary>
        public static IRevoConfiguration UseEFCoreInfrastructure(this IRevoConfiguration configuration,
            Action<EFCoreInfrastructureConfigurationSection> advancedAction = null,
            bool useDefaults = true)
        {
            configuration
                .ConfigureInfrastructure()
                .UseEFCoreDataAccess(null);

            configuration.ConfigureEvents();

            var section = configuration.GetOrAddSection<EFCoreInfrastructureConfigurationSection>(cfg =>
            {
                var s = new EFCoreInfrastructureConfigurationSection(useDefaults);
                cfg.ConfigureServices(services =>
                {
                    if (s.UseCrudAggregateStore || s.UseEventSourcedAggregateStore || s.UseProjectionsEnabled)
                    {
                        RegisterRepositoriesAndCoordinator(services, s);
                    }

                    if (s.UseEventStoreEnabled)
                    {
                        RegisterEventStore(services);
                    }

                    if (s.UseAsyncEventsEnabled)
                    {
                        RegisterAsyncEvents(services, s);
                    }

                    if (s.UseProjectionsEnabled)
                    {
                        RegisterProjections(services, s);
                    }

                    if (s.UseSagasEnabled)
                    {
                        RegisterSagas(services);
                    }
                });
                cfg.AddAfterConfigureServicesAction(services =>
                {
                    if (s.UseProjectionsEnabled && s.AutoDiscoverProjectors)
                    {
                        EFCoreProjectorDiscovery.DiscoverProjectors(cfg, services);
                    }
                });
                return s;
            });

            advancedAction?.Invoke(section);

            return configuration;
        }

        private static void RegisterRepositoriesAndCoordinator(IServiceCollection services,
            EFCoreInfrastructureConfigurationSection section)
        {
            if (section.UseCrudAggregateStore)
            {
                services.AddTransient<IAggregateStoreFactory, EFCoreCrudAggregateStoreFactory>();
            }

            if (section.UseEventSourcedAggregateStore)
            {
                services.AddTransient<IAggregateStoreFactory, EFCoreEventSourcedAggregateStoreFactory>();
            }

            services.AddScoped<EFCoreCoordinatedTransaction>();
            services.AddScoped<IEFCoreTransactionCoordinator>(sp => sp.GetRequiredService<EFCoreCoordinatedTransaction>());
            services.AddScoped<IUnitOfWorkListener>(sp => sp.GetRequiredService<EFCoreCoordinatedTransaction>());
        }

        private static void RegisterEventStore(IServiceCollection services)
        {
            services.AddScoped<IEventStore, EFCoreEventStore>();
            services.AddScoped<IExternalEventStore, EFCoreExternalEventStore>();
            services.AddTransient<IEventSourceCatchUp, EventSourceCatchUp>();
            services.AddTransient<IEventSourceCatchUp, ExternalEventSourceCatchUp>();
            services.AddSingleton(new ResourceDatabaseMigrationDiscoveryAssembly(
                typeof(InfrastructureConfigurationSection).Assembly, "Sql"));
        }

        private static void RegisterAsyncEvents(IServiceCollection services, EFCoreInfrastructureConfigurationSection section)
        {
            services.AddScoped<IAsyncEventQueueManager, EFCoreAsyncEventQueueManager>();
            services.TryAddScoped<IAsyncEventQueueDispatcher, AsyncEventQueueDispatcher>();
            services.AddSingleton<IEventSerializer>(sp =>
                new EventSerializer(sp.GetRequiredService<IVersionedTypeRegistry>(), section.CustomizeEventJsonSerializer));
            services.AddSingleton<IQueuedAsyncEventMessageFactory, QueuedAsyncEventMessageFactory>();
            services.AddSingleton(new ResourceDatabaseMigrationDiscoveryAssembly(
                typeof(InfrastructureConfigurationSection).Assembly, "Sql"));
        }

        private static void RegisterProjections(IServiceCollection services, EFCoreInfrastructureConfigurationSection section)
        {
            services.BindAsyncEventListener<EFCoreProjectionEventListener, EFCoreProjectionEventListener.EFCoreProjectionEventSequencer>();
            services.AddScoped<IEFCoreProjectionSubSystem, EFCoreProjectionSubSystem>();
            services.AddTransient<IEFCoreProjectorResolver, EFCoreProjectorResolver>();
            services.AddScoped<EFCoreSyncProjectionHook>();

        }

        private static void RegisterSagas(IServiceCollection services)
        {
            services.AddScoped<ISagaMetadataRepository, SagaMetadataRepository>();
            services.AddSingleton(new ResourceDatabaseMigrationDiscoveryAssembly(
                typeof(InfrastructureConfigurationSection).Assembly, "Sql"));
        }
    }
}
