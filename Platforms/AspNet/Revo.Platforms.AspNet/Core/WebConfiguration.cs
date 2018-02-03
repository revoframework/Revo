using System.Web.Configuration;

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
