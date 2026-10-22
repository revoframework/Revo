using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.Configuration;

namespace Revo.Hangfire
{
    public static class HangfireConfigurationExtensions
    {
        public static IRevoConfiguration UseHangfire(this IRevoConfiguration configuration,
            Func<JobStorage> jobStorage = null,
            Action<HangfireConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureJobs();
            
            var section = configuration.GetOrAddSection<HangfireConfigurationSection>(cfg =>
            {
                var s = new HangfireConfigurationSection();
                s.IsActive = true;
                cfg.ConfigureServices(services =>
                {
                    if (s.IsActive)
                    {
                        services.AddSingleton(s);
                        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();
                    }
                });
                return s;
            });
            section.JobStorage = jobStorage ?? section.JobStorage;

            advancedAction?.Invoke(section);

            return configuration;
        }
    }
}
