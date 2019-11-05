using Revo.Core.Configuration;

namespace Revo.Extensions.History.Configuration
{
    public class HistoryConfigurationSection : IRevoConfigurationSection
    {
        public bool IsChangeTrackingActive { get; set; }
    }
}