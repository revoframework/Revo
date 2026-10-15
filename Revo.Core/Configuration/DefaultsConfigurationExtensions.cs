using System;
using Revo.Core.Commands.Configuration;
using Revo.Core.Security.Configuration;

namespace Revo.Core.Configuration;

public static class DefaultsConfigurationExtensions
{
    public static IRevoConfiguration ConfigureDefaults(this IRevoConfiguration configuration,
        Action<CommandsConfigurationSection> advancedAction = null)
    {
        return configuration
            .ConfigureCore()
            .ConfigureSecurity()
            .ConfigureServices()
            .ConfigureCommands();
    }
}