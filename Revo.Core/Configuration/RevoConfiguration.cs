using System;
using System.Collections.Generic;

namespace Revo.Core.Configuration
{
    public class RevoConfiguration : IRevoConfiguration
    {
        private readonly Dictionary<Type, IRevoConfigurationSection> sections =
            new Dictionary<Type, IRevoConfigurationSection>();

        public T GetSection<T>() where T : class, IRevoConfigurationSection, new()
        {
            if (sections.TryGetValue(typeof(T), out var section))
            {
                return (T) section;
            }

            var newSection = new T();
            sections[typeof(T)] = newSection;
            return newSection;
        }
    }
}
