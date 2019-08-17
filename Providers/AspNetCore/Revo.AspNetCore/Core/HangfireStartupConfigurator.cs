using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services
                .AddHangfire(globalCfg =>
                {
                    globalCfg.UseStorage(hangfireConfigurationSection.JobStorage);

                    foreach (var action in hangfireConfigurationSection.ConfigurationActions)
                    {
                        action(globalCfg);
                    }

                    globalCfg.UseNLogLogProvider();
                    globalCfg.UseActivator(new HangfireJobActivator(kernel));
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (hangfireConfigurationSection.UseDashboard)
            {
                app.UseHangfireDashboard();
            }

            app.UseHangfireServer();
        }
    }
}
