using System;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Jobs.Configuration;

public static class JobsConfigurationExtensions
{
    public static IRevoConfiguration ConfigureJobs(this IRevoConfiguration configuration,
        Action<JobsConfigurationSection> advancedAction = null)
    {
        var section = configuration.GetOrAddSection<JobsConfigurationSection>(cfg =>
        {
            var section = new JobsConfigurationSection();

            cfg.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddTransient<IJobHandler<IExecuteCommandJob>, ExecuteCommandJobHandler>();
                serviceCollection.AddSingleton<IJobRunner, JobRunner>();
            });
            
            return section;
        });
            
        advancedAction?.Invoke(section);

        return configuration;
    }
}