using System.Web.Helpers;
using System.Web.Http;
using System.Web.OData.Extensions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Revo.AspNet.Globalization;
using Revo.AspNet.IO;
using Revo.AspNet.Web;

namespace Revo.AspNet
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
