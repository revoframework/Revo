using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Server;
using Revo.Core.Configuration;

namespace Revo.Hangfire
{
    public class HangfireConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public Func<JobStorage> JobStorage { get; set; } = () => new MemoryStorage();
        public bool UseDashboard { get; set; } = false;
        public bool AddHangfireServer { get; set; } = true;
        public Action<IServiceProvider, BackgroundJobServerOptions> BackgroundJobServerOptionsAction { get; set; } = (serviceProvider, options) => { };
        public List<IBackgroundProcess> AdditionalProcesses { get; set; } = new();
        public List<Action<IGlobalConfiguration>> ConfigurationActions { get; set; } = new();
    }
}
