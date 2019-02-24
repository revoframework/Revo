using System.Web.Http;
using Revo.AspNet.Web;
using Revo.Core.Configuration;

namespace Revo.AspNet
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IRevoConfiguration revoConfiguration)
        {
            var aspNetConfig = revoConfiguration.GetSection<AspNetConfigurationSection>();

            if (aspNetConfig.UseODataExtensions)
            {
                config.Filters.Add(new ODataActionFilterAttribute()); //also applies the OData filters (instead of a global AddODataQueryFilter(...)
            }
        }
    }
}
