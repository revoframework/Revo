using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Revo.Platforms.AspNet.Globalization;
using Revo.Platforms.AspNet.IO;
using Revo.Platforms.AspNet.Web;

namespace Revo.Platforms.AspNet
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            config.Filters.Add(new ODataActionFilterAttribute()); //also applies the OData filters (instead of a global AddODataQueryFilter(...)
            config.Select().Expand().Filter().OrderBy().MaxTop(null).Count(); //enable common OData options
            config.EnableDependencyInjection();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new BracesGuidJsonConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new TranslatingJsonConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            config.Services.Replace(typeof(IHttpActionSelector), new HyphenApiControllerActionSelector());
            config.Services.Replace(typeof(IHttpControllerSelector), new ApiControllerSelector(config));
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new ApiControllerTypeResolver());

            config.Filters.Add(new ValidateApiActionModelFilterAttribute());
            //config.Filters.Add(new ValidateHttpAntiForgeryTokenAttribute());

            AntiForgeryConfig.CookieName = AntiForgeryConsts.CookieTokenName;
        }
    }
}
