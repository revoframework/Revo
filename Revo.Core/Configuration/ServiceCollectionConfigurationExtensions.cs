using System;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Configuration;

public static class ServiceCollectionConfigurationExtensions
{
    public static IRevoConfiguration ConfigureServices(this IRevoConfiguration configuration,
        Action<IServiceCollection> configureAction = null)
    {
        var section = configuration.GetOrAddSection<ServiceConfigurationSection>(_ => new ServiceConfigurationSection());

        if (configureAction != null)
        {
            section.AddConfigureServicesAction(configureAction);
        }

        return configuration;
    }
    
    public static IRevoConfiguration AddAfterConfigureServicesAction(this IRevoConfiguration configuration,
        Action<IServiceCollection> configureAction = null)
    {
        var section = configuration.GetOrAddSection<ServiceConfigurationSection>(_ => new ServiceConfigurationSection());

        if (configureAction != null)
        {
            section.AddAfterConfigureServicesAction(configureAction);
        }

        return configuration;
    }
}