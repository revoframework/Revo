using System;
using Revo.Core.Configuration;

namespace Revo.Extensions.AutoMapper.Configuration
{
    public static class AutoMapperConfigurationExtensions
    {
        public static IRevoConfiguration AddAutoMapperExtension(this IRevoConfiguration configuration,
            Action<AutoMapperConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<AutoMapperConfigurationSection>();
            section.AutoDiscoverProfiles = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.AutoDiscoverProfiles)
                {
                    c.LoadModule(new AutoMapperModule());
                }
            });

            return configuration;
        }
    }
}