using Hangfire;
using Owin;
using Revo.AspNet.Core.Lifecycle;
using Revo.Hangfire;

namespace Revo.AspNet.Core
{
    public class HangfireOwinConfigurator : IOwinConfigurator
    {
        private readonly HangfireConfigurationSection hangfireConfigurationSection;

        public HangfireOwinConfigurator(HangfireConfigurationSection hangfireConfigurationSection)
        {
            this.hangfireConfigurationSection = hangfireConfigurationSection;
        }
        
        public void ConfigureApp(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseStorage(hangfireConfigurationSection.JobStorage());

            if (hangfireConfigurationSection.UseDashboard)
            {
                app.UseHangfireDashboard();
            }

            app.UseHangfireServer();

            foreach (var action in hangfireConfigurationSection.ConfigurationActions)
            {
                action(GlobalConfiguration.Configuration);
            }
        }
    }
}
