using System;
using Rebus.Config;
using Revo.Core.Configuration;

namespace Revo.Rebus
{
    public static class RebusConfigurationExtensions
    {
        /// <summary>
        /// Uses Rebus service bus integration.
        /// </summary>
        /// <param name="configureFunc">Function to configure Rebus. You have to configure your connection, routing, etc. here.</param>
        /// <param name="advancedAction">Advanced extension configuration action (optional).</param>
        /// <returns></returns>
        public static IRevoConfiguration UseRebus(this IRevoConfiguration configuration,
            Func<RebusConfigurer, RebusConfigurer> configureFunc,
            Action<RebusConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<RebusConfigurationSection>();
            section.IsActive = true;
            section.ConfigureFunc = section.ConfigureFunc;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new RebusModule(section.ConfigureFunc));
                }
            });

            return configuration;
        }
    }
}
