using System;

namespace Revo.Core.Configuration;

public interface IRevoConfiguration
{
    T GetSection<T>() where T : class, IRevoConfigurationSection;
    T GetOrAddSection<T>(Func<IRevoConfiguration, T> factory) where T : class, IRevoConfigurationSection;
}