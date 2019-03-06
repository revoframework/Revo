using System;
using Revo.Core.Configuration;

namespace Revo.EasyNetQ.Configuration
{
    public static class EasyNetQConfigurationExtensions
    {
        public static IRevoConfiguration UseEasyNetQ(this IRevoConfiguration configuration,
            bool? isActive = true,
            EasyNetQConnectionConfiguration connection = null,
            Action<EasyNetQSubscriptionsConfiguration> subscriptions = null,
            Action<EasyNetQConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EasyNetQConfigurationSection>();
            section.IsActive = isActive ?? section.IsActive;
            section.Connection = connection ?? section.Connection;

            subscriptions?.Invoke(section.Subscriptions);
            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {

                c.LoadModule(new EasyNetQModule(section));
            });

            return configuration;
        }
    }
}
