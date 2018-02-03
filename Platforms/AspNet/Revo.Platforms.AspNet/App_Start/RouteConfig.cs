using System.Web.Mvc;
using System.Web.Routing;
using LowercaseDashedRouting;

namespace Revo.Platforms.AspNet
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            /*routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );*/

            routes.Add(new LowercaseDashedRoute("{controller}/{action}/{id}",
                new RouteValueDictionary(
                    new { controller = "Home", action = "Index", id = UrlParameter.Optional }),
                    new DashedRouteHandler()
                )
            );
        }
    }
}
