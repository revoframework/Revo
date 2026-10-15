using System;
using Revo.Core.Configuration;

namespace Revo.Infrastructure.Tenancy.Configuration;

public static class TenancyConfigurationExtensions
{
    public static IRevoConfiguration ConfigureTenancy(this IRevoConfiguration configuration,
        Action<TenancyConfigurationSection> advancedAction = null)
    {
        var section = configuration.GetOrAddSection<TenancyConfigurationSection>(configuration =>
        {
            var section = new TenancyConfigurationSection();
            return section;
        });
            
        advancedAction?.Invoke(section);

        return configuration;
    }
}
