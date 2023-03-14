using System;
using Hangfire;
using Revo.Core.Configuration;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Hangfire
{
    public static class HangfireConfigurationExtensions
    {
        public static IRevoConfiguration UseHangfire(this IRevoConfiguration configuration,
            Func<JobStorage> jobStorage = null,
            Action<HangfireConfigurationSection> advancedAction = null)
        {
            configuration.UseInMemoryJobs(false);

            var section = configuration.GetSection<HangfireConfigurationSection>();
            section.IsActive = true;
            section.JobStorage = jobStorage ?? section.JobStorage;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new HangfireModule(section));
                }
            });

            return configuration;
        }
    }
}
