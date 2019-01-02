using System;
using System.Collections.Generic;
using System.Text;

namespace Revo.Core.Configuration
{
    public class CoreConfigurationSection : IRevoConfigurationSection
    {
        public bool AutoDiscoverCommandHandlers { get; set; } = true;
        public bool AutoDiscoverAutoMapperProfiles { get; set; } = true;
    }
}
