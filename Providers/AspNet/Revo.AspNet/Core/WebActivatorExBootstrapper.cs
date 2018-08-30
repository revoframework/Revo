using Revo.AspNet.Core;
using Revo.AspNet.Core.Lifecycle;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(WebActivatorExBootstrapper), "OnPostApplicationStart")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(WebActivatorExBootstrapper), "OnApplicationShutdown")]
namespace Revo.AspNet.Core
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
