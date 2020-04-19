using System;
using Revo.Core.Commands;
using Revo.Core.Core;

namespace Revo.Core.Configuration
{
    public static class CoreConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureCore(this IRevoConfiguration configuration,
            Action<CoreConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<CoreConfigurationSection>();

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                c.LoadModule(new CoreModule(section));
                c.LoadModule(new CommandsModule(section.Commands));
            });

            return configuration;
        }
    }
}
