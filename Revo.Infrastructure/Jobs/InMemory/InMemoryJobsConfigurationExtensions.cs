using System;
using Revo.Core.Configuration;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public static class InMemoryJobsConfigurationExtensions
    {
        public static IRevoConfiguration UseInMemoryJobs(this IRevoConfiguration configuration,
            bool? isActive = true,
            Action<InMemoryJobsConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<InMemoryJobsConfigurationSection>();

            section.IsActive = isActive ?? (section.IsActive ?? true);

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                c.LoadModule(new InMemoryJobsModule(section.SchedulerConfiguration, section.IsActive ?? true));
            });

            return configuration;
        }
    }
}
