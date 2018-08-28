using Revo.Core.Lifecycle;
using Revo.Platforms.AspNet.Core;
using Revo.Platforms.AspNet.Core.Lifecycle;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(WebActivatorExBootstrapper), "OnPostApplicationStart")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(WebActivatorExBootstrapper), "OnApplicationShutdown")]
namespace Revo.Platforms.AspNet.Core
{
    public static class WebActivatorExBootstrapper
    {
        public static void OnPostApplicationStart()
        {
            foreach (IWebActivatorExHooks appInitializer in RevoHttpApplication.Current.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnPostApplicationStart();
            }
            
            RevoHttpApplication.Current.PostStart();
        }

        public static void OnApplicationShutdown()
        {
            foreach (IWebActivatorExHooks appInitializer in RevoHttpApplication.Current.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnApplicationShutdown();
            }
        }
    }
}
