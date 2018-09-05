using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Revo.Hangfire;

namespace Revo.AspNetCore.Core
{
    public class HangfireStartupConfigurator : IAspNetCoreStartupConfigurer
    {
        private readonly HangfireConfigurationSection hangfireConfigurationSection;

        public HangfireStartupConfigurator(HangfireConfigurationSection hangfireConfigurationSection)
        {
            this.hangfireConfigurationSection = hangfireConfigurationSection;
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
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (hangfireConfigurationSection.UseDashboard)
            {
                app.UseHangfireDashboard();
            }

            app.UseHangfireServer();
        }
    }
}
