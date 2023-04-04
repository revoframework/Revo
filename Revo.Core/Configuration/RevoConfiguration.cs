using System;
using System.Collections.Generic;

namespace Revo.Core.Configuration;

public class RevoConfiguration : IRevoConfiguration
{
    private readonly Dictionary<Type, IRevoConfigurationSection> sections = new();

    public T GetSection<T>() where T : class, IRevoConfigurationSection
    {
        if (sections.TryGetValue(typeof(T), out var section))
        {
            return (T) section;
        }

        throw new InvalidOperationException($"Configuration section {typeof(T).FullName} not added yet");
    }

    public T GetOrAddSection<T>(Func<IRevoConfiguration, T> factory) where T : class, IRevoConfigurationSection
    {
        if (sections.TryGetValue(typeof(T), out var section))
        {
            return (T) section;
        }

        var newSection = factory(this);
        sections[typeof(T)] = newSection;
        return newSection;
    }
}