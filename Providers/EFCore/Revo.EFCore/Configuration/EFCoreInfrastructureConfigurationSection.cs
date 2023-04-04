using System;
using Newtonsoft.Json;
using Revo.Core.Configuration;

namespace Revo.EFCore.Configuration
{
    public class EFCoreInfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public EFCoreInfrastructureConfigurationSection(bool useDefaults = true)
        {
            if (useDefaults)
            {
                UseEventStoreEnabled = true;
                UseSagasEnabled = true;
                UseAsyncEventsEnabled = true;
                UseProjectionsEnabled = true;
                UseEventSourcedAggregateStore = true;
                UseCrudAggregateStore = true;
                AutoDiscoverProjectors = true;
            }
        }

        /// <summary>When true, registers EF Core event store services.</summary>
        public bool UseEventStoreEnabled { get; set; }

        /// <summary>When true, registers EF Core saga metadata repository.</summary>
        public bool UseSagasEnabled { get; set; }

        /// <summary>When true, registers async event queue manager and serializers.</summary>
        public bool UseAsyncEventsEnabled { get; set; }

        /// <summary>When true, registers projection subsystem and listeners.</summary>
        public bool UseProjectionsEnabled { get; set; }

        public bool UseEventSourcedAggregateStore { get; set; }
        public bool UseCrudAggregateStore { get; set; }
        public bool AutoDiscoverProjectors { get; set; }

        public Func<JsonSerializerSettings, JsonSerializerSettings> CustomizeEventJsonSerializer { get; set; } = settings => settings;

        public EFCoreInfrastructureConfigurationSection UseEventStore(bool enable = true)
        {
            UseEventStoreEnabled = enable;
            return this;
        }

        public EFCoreInfrastructureConfigurationSection UseRepositories(
            bool useCrudAggregateStore = true,
            bool useEventSourcedAggregateStore = true)
        {
            UseCrudAggregateStore = useCrudAggregateStore;
            UseEventSourcedAggregateStore = useEventSourcedAggregateStore;
            return this;
        }

        public EFCoreInfrastructureConfigurationSection UseSagas(bool enable = true)
        {
            UseSagasEnabled = enable;
            return this;
        }

        public EFCoreInfrastructureConfigurationSection UseProjections(bool enable = true, bool autoDiscoverProjectors = true)
        {
            UseProjectionsEnabled = enable;
            AutoDiscoverProjectors = autoDiscoverProjectors;
            return this;
        }

        public EFCoreInfrastructureConfigurationSection UseAsyncEvents(bool enable = true)
        {
            UseAsyncEventsEnabled = enable;
            return this;
        }
    }
}
