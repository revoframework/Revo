using System.Web.Configuration;
using GTRevo.Core.Core;

namespace GTRevo.Platform.Core
{
    public class WebConfiguration : IConfiguration
    {
        public T GetSection<T>(string sectionName)
        {
            return (T)WebConfigurationManager.GetSection(sectionName);
        }
    }
}
