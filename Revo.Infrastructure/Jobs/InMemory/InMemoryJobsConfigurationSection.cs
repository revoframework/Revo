using System;
using System.Collections.Generic;
using System.Text;
using Revo.Core.Configuration;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public class InMemoryJobsConfigurationSection : IRevoConfigurationSection
    {
        public bool? IsActive { get; set; }
        public InMemoryJobSchedulerConfiguration SchedulerConfiguration { get; set; } = new InMemoryJobSchedulerConfiguration();
    }
}
