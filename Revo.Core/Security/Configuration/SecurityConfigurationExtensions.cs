using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Revo.Core.Configuration;
using Revo.Core.Security.ClaimBased;

namespace Revo.Core.Security.Configuration;

public static class SecurityConfigurationExtensions
{
    public static IRevoConfiguration ConfigureSecurity(this IRevoConfiguration configuration,
        Action<SecurityConfigurationSection> advancedAction = null)
    {
        var section = configuration.GetOrAddSection<SecurityConfigurationSection>(cfg =>
        {
            var section = new SecurityConfigurationSection();

            cfg.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddScoped<IPermissionAuthorizationMatcher, PermissionAuthorizationMatcher>();
                serviceCollection.AddScoped<IUserPermissionAuthorizer, UserPermissionAuthorizer>();
                
                // default null implementations
                serviceCollection.TryAddSingleton<IClaimsPrincipalUserResolver, NullClaimsPrincipalUserResolver>();
                serviceCollection.TryAddSingleton<IUserPermissionResolver, NullUserPermissionResolver>();
            });
            
            return section;
        });

        advancedAction?.Invoke(section);
        
        return configuration;
    }
}