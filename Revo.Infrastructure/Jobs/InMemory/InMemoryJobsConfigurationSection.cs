using Revo.Core.Configuration;

namespace Revo.Infrastructure.Jobs.InMemory;

public class InMemoryJobsConfigurationSection : IRevoConfigurationSection
{
    public InMemoryJobSchedulerConfiguration SchedulerConfiguration { get; set; } = new();
}