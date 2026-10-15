using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Revo.Core.Configuration;
using Revo.Core.Services;
using Revo.Infrastructure.Jobs.Configuration;

namespace Revo.Infrastructure.Jobs.InMemory;

public static class InMemoryJobsConfigurationExtensions
{
    public static IRevoConfiguration UseInMemoryJobs(this IRevoConfiguration configuration,
        Action<InMemoryJobsConfigurationSection> advancedAction = null)
    {
        configuration.ConfigureJobs();
        
        var section = configuration.GetOrAddSection<InMemoryJobsConfigurationSection>(cfg =>
        {
            var section = new InMemoryJobsConfigurationSection();

            cfg.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton<IInMemoryJobSchedulerConfiguration>(section.SchedulerConfiguration);
                serviceCollection.AddSingleton<IInMemoryJobWorkerProcess, IHostedService, InMemoryJobWorkerProcess>();
                serviceCollection.AddSingleton<IInMemoryJobSchedulerProcess, IHostedService, InMemoryJobSchedulerProcess>();
                serviceCollection.AddSingleton<IInMemoryJobScheduler, InMemoryJobScheduler>();
                
                serviceCollection.TryAddSingleton<IJobScheduler>(sc => sc.GetRequiredService<IInMemoryJobScheduler>());
            });
            
            return section;
        });
            
        advancedAction?.Invoke(section);

        return configuration;
    }
}