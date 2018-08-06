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
            config.Filters.Add(new ODataActionFilterAttribute()); //also applies the OData filters (instead of a global AddODataQueryFilter(...)
            config.Select().Expand().Filter().OrderBy().MaxTop(null).Count(); //enable common OData options
            config.EnableDependencyInjection();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            };

            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new BracesGuidJsonConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new TranslatingJsonConverter());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            
            config.Filters.Add(new ValidateApiActionModelFilterAttribute());
            //config.Filters.Add(new ValidateHttpAntiForgeryTokenAttribute()); //default off for now

            AntiForgeryConfig.CookieName = AntiForgeryConsts.CookieTokenName;
        }
    }
}
