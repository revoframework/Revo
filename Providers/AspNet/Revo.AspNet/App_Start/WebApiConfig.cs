using System.Web.Http;
using Revo.AspNet.Web;
using Revo.Core.Configuration;

namespace Revo.AspNet
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IRevoConfiguration revoConfiguration)
        {
            //var aspNetConfig = revoConfiguration.GetSection<AspNetConfigurationSection>();
            //nothing to configure right now (e.g. filters)
        }
    }
}
