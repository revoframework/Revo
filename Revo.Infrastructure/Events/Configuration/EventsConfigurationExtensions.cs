using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Revo.Core.Configuration;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Services;
using Revo.Core.Transactions;
using Revo.Core.Types;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Metadata;
using Revo.Infrastructure.Events.Upgrades;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Events.Configuration;

public static class EventsConfigurationExtensions
{
    public static IRevoConfiguration ConfigureEvents(this IRevoConfiguration configuration,
        Action<EventsConfigurationSection> advancedAction = null)
    {
        var section = configuration.GetOrAddSection<EventsConfigurationSection>(cfg =>
        {
            var eventsSection = new EventsConfigurationSection();

            cfg.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddScoped<IEventMessageFactory, EventMessageFactory>();
                serviceCollection.AddScoped<IEventMetadataProvider, ActorNameEventMetadataProvider>();
                serviceCollection.AddScoped<IEventMetadataProvider, MachineNameEventMetadataProvider>();
                serviceCollection.AddScoped<IEventMetadataProvider, UserIdEventMetadataProvider>();
                serviceCollection.AddScoped<IEventMetadataProvider, CommandContextEventMetadataProvider>();
                serviceCollection.TryAddSingleton<IEventStreamSequenceNumbersUpgrade, EventStreamSequenceNumbersUpgrade>();
                serviceCollection.TryAddSingleton<IEventStreamUpgrader, EventStreamUpgrader>();
                serviceCollection.AddSingleton<IAsyncEventWorkerLockCache, AsyncEventWorkerLockCache>();
                serviceCollection.AddScoped<AsyncEventWorker>();
                serviceCollection.AddScoped<IAsyncEventWorker>(sp => new LockingAsyncEventWorker(
                    sp.GetRequiredService<AsyncEventWorker>(),
                    sp.GetRequiredService<IAsyncEventWorkerLockCache>()));
                serviceCollection.TryAddScoped<IAsyncEventQueueDispatcher, AsyncEventQueueDispatcher>();
                serviceCollection.AddScoped<IEventListener<IEvent>, IUnitOfWorkListener, AsyncEventQueueDispatchListener>();
                serviceCollection.AddScoped<IAsyncEventProcessor, AsyncEventProcessor>();
                serviceCollection.AddScoped<IJobHandler<ProcessAsyncEventsJob>, ProcessAsyncEventsJobHandler>();
                serviceCollection.AddHostedService<AsyncEventExecutionCatchUp>();
            });

            cfg.AddAfterConfigureServicesAction(serviceCollection =>
            {
                if (eventsSection.AutoDiscoverEventUpgrades)
                {
                    new EventUpgradeDiscovery(cfg, NullLogger<EventUpgradeDiscovery>.Instance)
                        .DiscoverEventUpgrades(serviceCollection);
                }
            });

            return eventsSection;
        });

        advancedAction?.Invoke(section);

        return configuration;
    }
}
