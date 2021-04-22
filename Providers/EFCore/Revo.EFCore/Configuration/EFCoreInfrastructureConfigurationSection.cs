using System;
using Newtonsoft.Json;
using Revo.Core.Configuration;

namespace Revo.EFCore.Configuration
{
    public class EFCoreInfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public bool AutoDiscoverProjectors { get; set; }
        public bool UseEventStore { get; set; }
        public bool UseSagas { get; set; }
        public bool UseAsyncEvents { get; set; }
        public bool UseProjections { get; set; }
        public bool UseEventSourcedAggregateStore { get; set; }
        public bool UseCrudAggregateStore { get; set; }

        public Func<JsonSerializerSettings, JsonSerializerSettings> CustomizeEventJsonSerializer { get; set; } = settings => settings;
    }
}
