using Microsoft.Owin;
using Owin;
using Revo.AspNet.Core;
using Revo.AspNet.Core.Lifecycle;

[assembly: OwinStartup(typeof(Startup))]
namespace Revo.AspNet.Core
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
