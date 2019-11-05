using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Revo.Core.Configuration;

namespace Revo.Hangfire
{
    public class HangfireConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public Func<JobStorage> JobStorage { get; set; } = () => new MemoryStorage();
        public bool UseDashboard { get; set; } = true;
        public Action<IGlobalConfiguration>[] ConfigurationActions { get; set; } = { };
    }
}
