using Revo.Core.Configuration;

namespace Revo.Infrastructure.Events.Configuration;

public class EventsConfigurationSection : IRevoConfigurationSection
{
    /// <summary>When true, registers all <see cref="Revo.Core.Events.IEventUpgrade"/> implementations in the type explorer.</summary>
    public bool AutoDiscoverEventUpgrades { get; set; } = true;
}
