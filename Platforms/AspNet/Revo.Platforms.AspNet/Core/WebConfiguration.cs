using System.Web.Configuration;
using Revo.Core.Core;

namespace Revo.Platforms.AspNet.Core
{
    public class WebConfiguration : IConfiguration
    {
        public T GetSection<T>(string sectionName)
        {
            return (T)WebConfigurationManager.GetSection(sectionName);
        }
    }
}
