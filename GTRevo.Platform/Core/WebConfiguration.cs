using System.Web.Configuration;

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
