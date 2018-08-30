using System;
using Hangfire;
using Revo.Core.Configuration;
using Revo.Infrastructure.Jobs;

namespace Revo.Hangfire
{
    public static class HangfireConfigurationExtensions
    {
        public static IRevoConfiguration UseHangfire(this IRevoConfiguration configuration,
            JobStorage jobStorage = null,
            Action<HangfireConfigurationSection> advancedAction = null)
        {
            configuration.OverrideModuleLoading<NullJobsModule>(false);

            var section = configuration.GetSection<HangfireConfigurationSection>();
            section.IsActive = true;
            section.JobStorage = jobStorage ?? section.JobStorage;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new HangfireModule());
                }
            });

            return configuration;
        }
    }
}
