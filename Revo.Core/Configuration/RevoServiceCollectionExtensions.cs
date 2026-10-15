using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Revo.Core.Configuration;

public static class RevoServiceCollectionExtensions
{
    public static IServiceCollection AddRevo(
        this IServiceCollection serviceCollection,
        Action<IRevoConfiguration> configurator = null)
    {
        var configuration = new RevoConfiguration();
        
        configuration.ConfigureCore();
        configurator?.Invoke(configuration);

        RunServiceRegistrators(serviceCollection, configuration);
        
        return serviceCollection;
    }

    private static void RunServiceRegistrators(IServiceCollection serviceCollection, RevoConfiguration configuration)
    {
        var serviceConfigurationSection = configuration.GetSection<ServiceConfigurationSection>();
        foreach (var action in serviceConfigurationSection.ConfigureServicesActions)
        {
            action(serviceCollection);
        }

        foreach (var action in serviceConfigurationSection.AfterConfigureServicesActions)
        {
            action(serviceCollection);
        }
    }
}