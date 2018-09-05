using Revo.Core.Configuration;

namespace Revo.AspNet
{
    public class AspNetConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public bool UseODataExtensions { get; set; }
    }
}
