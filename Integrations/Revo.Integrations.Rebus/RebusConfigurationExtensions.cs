using System;
using Revo.Core.Configuration;

namespace Revo.Integrations.Rebus
{
    public static class RebusConfigurationExtensions
    {
        public static IRevoConfiguration UseRebus(this IRevoConfiguration configuration,
            bool? useAsPrimaryRepository = true,
            RebusConnectionConfiguration connection = null,
            Action<RebusConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<RebusConfigurationSection>();
            section.IsActive = true;
            section.Connection = connection ?? section.Connection;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new RebusModule(section.Connection));
                }
            });

            return configuration;
        }
    }
}
