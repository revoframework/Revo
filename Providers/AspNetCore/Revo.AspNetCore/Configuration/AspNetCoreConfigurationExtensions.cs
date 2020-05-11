using System;
using Revo.AspNetCore.Security;
using Revo.Core.Configuration;
using Revo.Hangfire;

namespace Revo.AspNetCore.Configuration
{
    public static class AspNetCoreConfigurationExtensions
    {
        public static IRevoConfiguration UseAspNetCore(this IRevoConfiguration configuration,
            Action<AspNetCoreConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<AspNetCoreConfigurationSection>();
            section.IsActive = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    var hangfireConfigurationSection = configuration.GetSection<HangfireConfigurationSection>();
                    c.LoadModule(new AspNetCoreModule(hangfireConfigurationSection));
                    c.LoadModule(new AspNetCoreSecurityModule());
                }
            });

            return configuration;
        }
    }
}
