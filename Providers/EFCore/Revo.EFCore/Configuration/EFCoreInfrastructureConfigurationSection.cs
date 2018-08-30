using Revo.Core.Configuration;

namespace Revo.EFCore.Configuration
{
    public class EFCoreInfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public bool UseEventStore { get; set; }
        public bool UseSagas { get; set; }
        public bool UseAsyncEvents { get; set; }
        public bool UseProjections { get; set; }
    }
}
