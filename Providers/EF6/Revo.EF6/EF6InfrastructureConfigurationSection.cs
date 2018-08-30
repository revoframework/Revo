using Revo.Core.Configuration;

namespace Revo.EF6
{
    public class EF6InfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public bool UseEventStore { get; set; }
        public bool UseSagas { get; set; }
        public bool UseAsyncEvents { get; set; }
        public bool UseProjections { get; set; }
    }
}
