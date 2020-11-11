using Rebus.Config;
using Revo.Core.Configuration;
using System;

namespace Revo.Rebus
{
    public class RebusConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public Func<RebusConfigurer, RebusConfigurer> ConfigureFunc { get; set; } = null;
    }
}
