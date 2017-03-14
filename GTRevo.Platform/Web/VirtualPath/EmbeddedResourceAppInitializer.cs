using GTRevo.Platform.Core.Lifecycle;

namespace GTRevo.Platform.Web.VirtualPath
{
    public class EmbeddedResourceAppInitializer : IWebActivatorExHooks
    {
        public EmbeddedResourceAppInitializer(EmbeddedResourceVirtualPathProvider provider)
        {
            Provider = provider;
        }

        public static EmbeddedResourceVirtualPathProvider Provider { get; private set; }

        public void OnPreApplicationStart()
        {
        }

        public void OnPostApplicationStart()
        {
            System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(Provider);
        }

        public void OnApplicationShutdown()
        {
        }
    }
}
