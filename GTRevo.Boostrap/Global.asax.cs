using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GTRevo.Platform;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;

namespace GTRevo.Boostrap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            foreach (IHttpApplicationInitializer appInitializer in NinjectWebLoader.ResolveAll<IHttpApplicationInitializer>())
            {
                appInitializer.OnApplicationStart(this);
            }
        }
    }
}
