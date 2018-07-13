using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Revo.Platforms.AspNet.Core.Lifecycle;

namespace Revo.Examples.HelloAspNet.Bootstrap
{
    public class AppStartHttpApplicationInitializer : IHttpApplicationInitializer
    {
        public void OnApplicationStart(HttpApplication application)
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
