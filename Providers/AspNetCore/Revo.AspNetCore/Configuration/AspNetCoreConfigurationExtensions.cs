using System;
using Microsoft.Extensions.DependencyInjection;
using Revo.AspNetCore.Core;
using Revo.AspNetCore.Security;
using Revo.Core.Configuration;
using Revo.Core.Core;
using Revo.Core.Security;

namespace Revo.AspNetCore.Configuration;

public static class AspNetCoreConfigurationExtensions
{
    public static IRevoConfiguration UseAspNetCore(this IRevoConfiguration configuration,
        Action<AspNetCoreConfigurationSection> advancedAction = null)
    {
        var section = configuration.GetOrAddSection<AspNetCoreConfigurationSection>(cfg =>
        {
            var section = new AspNetCoreConfigurationSection();

            cfg.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddScoped<IActorContext, UserActorContext>();
                serviceCollection.AddSingleton<IEnvironmentProvider, AspNetCoreEnvironmentProvider>();
                serviceCollection.AddScoped<IUserContext, AspNetCoreUserContext>();
                serviceCollection.AddHttpContextAccessor();
            });
            
            return section;
        });
        
        advancedAction?.Invoke(section);
        
        return configuration;
    }
}