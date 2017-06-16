using GTRevo.Boostrap.Core;
using GTRevo.Core.Lifecycle;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(WebActivatorExBootstrapper), "OnPreApplicationStart")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(WebActivatorExBootstrapper), "OnPostApplicationStart")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(WebActivatorExBootstrapper), "OnApplicationShutdown")]
namespace GTRevo.Boostrap.Core
{
    public static class WebActivatorExBootstrapper
    {
        public static void OnPreApplicationStart()
        {
            NinjectWebLoader.Start();
            
            foreach (IWebActivatorExHooks appInitializer in NinjectWebLoader.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnPreApplicationStart();
            }
        }

        public static void OnPostApplicationStart()
        {
            NinjectWebLoader.PostStart();

            foreach (IWebActivatorExHooks appInitializer in NinjectWebLoader.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnPostApplicationStart();
            }

            foreach (IApplicationStartListener startListener in NinjectWebLoader.ResolveAll<IApplicationStartListener>())
            {
                startListener.OnApplicationStarted();
            }
        }

        public static void OnApplicationShutdown()
        {

            foreach (IApplicationStopListener stopListener in NinjectWebLoader.ResolveAll<IApplicationStopListener>())
            {
                stopListener.OnApplicationStopping();
            }

            foreach (IWebActivatorExHooks appInitializer in NinjectWebLoader.ResolveAll<IWebActivatorExHooks>())
            {
                appInitializer.OnApplicationShutdown();
            }

            NinjectWebLoader.Stop();
        }
    }
}
