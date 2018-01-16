using GTRevo.Boostrap;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace GTRevo.Boostrap
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            foreach (IOwinConfigurator owinConfigurator in RevoHttpApplication.Current.ResolveAll<IOwinConfigurator>())
            {
                owinConfigurator.ConfigureApp(app);
            }
        }
    }
}
