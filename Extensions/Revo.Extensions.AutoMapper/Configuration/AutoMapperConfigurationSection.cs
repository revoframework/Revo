using Revo.Core.Configuration;

namespace Revo.Extensions.AutoMapper.Configuration
{
    public class AutoMapperConfigurationSection : IRevoConfigurationSection
    {
        public bool AutoDiscoverProfiles { get; set; }
    }
}