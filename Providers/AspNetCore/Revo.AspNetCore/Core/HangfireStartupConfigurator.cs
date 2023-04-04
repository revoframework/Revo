using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Revo.Hangfire;

namespace Revo.AspNetCore.Core
{
    public class HangfireStartupConfigurator : IAspNetCoreStartupConfigurer
    {
        private readonly HangfireConfigurationSection hangfireConfigurationSection;
        private readonly IKernel kernel;

        public HangfireStartupConfigurator(HangfireConfigurationSection hangfireConfigurationSection,
            IKernel kernel)
        {
            this.hangfireConfigurationSection = hangfireConfigurationSection;
            this.kernel = kernel;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var storage = hangfireConfigurationSection.JobStorage();

            // this needs to be set here, otherwise Hangfire throws error when enqueuing a job
            // before IGlobalConfiguration has been first requested by the DI container
            GlobalConfiguration.Configuration.UseStorage(storage);
            
            services
                .AddHangfire(configuration =>
                {
                    configuration.UseActivator(new HangfireJobActivator(kernel));
                    
                    foreach (var action in hangfireConfigurationSection.ConfigurationActions)
                    {
                        action(configuration);
                    }
                });

            if (hangfireConfigurationSection.AddHangfireServer)
            {
                services
                    .AddHangfireServer(hangfireConfigurationSection.BackgroundJobServerOptionsAction,
                        storage, hangfireConfigurationSection.AdditionalProcesses.ToArray());
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (hangfireConfigurationSection.UseDashboard)
            {
                app.UseHangfireDashboard();
            }
        }
    }
}
