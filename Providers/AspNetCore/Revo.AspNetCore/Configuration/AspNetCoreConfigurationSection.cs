using Revo.Core.Configuration;

namespace Revo.AspNetCore.Configuration
{
    public class AspNetCoreConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
    }
}
