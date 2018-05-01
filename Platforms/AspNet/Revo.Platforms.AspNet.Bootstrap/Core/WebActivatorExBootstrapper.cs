using System.Linq;
using Revo.Core.Lifecycle;
using Revo.Platforms.AspNet.Boostrap.Core;
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

            var initializer = RevoHttpApplication.Current.Resolve<IApplicationStartListenerInitializer>();
            initializer.InitializeStarted();
        }

        public static void OnApplicationShutdown()
        {
            var initializer = RevoHttpApplication.Current.Resolve<IApplicationStartListenerInitializer>();
            initializer.DeinitializeStopping();

            foreach (IWebActivatorExHooks appInitializer in RevoHttpApplication.Current.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnApplicationShutdown();
            }
        }
    }
}
