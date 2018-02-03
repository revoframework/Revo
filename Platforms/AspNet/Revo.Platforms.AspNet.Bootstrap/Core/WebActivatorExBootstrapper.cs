using System.Linq;
using Revo.Platforms.AspNet.Boostrap.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Platforms.AspNet.Core;
using Revo.Platforms.AspNet.Core.Lifecycle;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(WebActivatorExBootstrapper), "OnPreApplicationStart")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(WebActivatorExBootstrapper), "OnPostApplicationStart")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(WebActivatorExBootstrapper), "OnApplicationShutdown")]
namespace Revo.Platforms.AspNet.Boostrap.Core
{
    public static class WebActivatorExBootstrapper
    {
        public static void OnPreApplicationStart()
        {
            /*foreach (IWebActivatorExHooks appInitializer in RevoHttpApplication.Current.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnPreApplicationStart();
            }*/
        }

        public static void OnPostApplicationStart()
        {
            RevoHttpApplication.Current.PostStart();

            foreach (IWebActivatorExHooks appInitializer in RevoHttpApplication.Current.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnPostApplicationStart();
            }
            
            foreach (IApplicationStartListener startListener in RevoHttpApplication.Current.ResolveAll<IApplicationStartListener>())
            {
                startListener.OnApplicationStarted();
            }
        }

        public static void OnApplicationShutdown()
        {

            foreach (IApplicationStopListener stopListener in RevoHttpApplication.Current.ResolveAll<IApplicationStopListener>())
            {
                stopListener.OnApplicationStopping();
            }

            foreach (IWebActivatorExHooks appInitializer in RevoHttpApplication.Current.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnApplicationShutdown();
            }
        }
    }
}
