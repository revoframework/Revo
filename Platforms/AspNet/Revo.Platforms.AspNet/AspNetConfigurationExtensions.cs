using System;
using Revo.Core.Configuration;
using Revo.Infrastructure.Hangfire;
using Revo.Platforms.AspNet.Core;
using Revo.Platforms.AspNet.Globalization;
using Revo.Platforms.AspNet.IO.Templates;
using Revo.Platforms.AspNet.Security;

namespace Revo.Platforms.AspNet
{
    public static class AspNetConfigurationExtensions
    {
        public static IRevoConfiguration UseAspNet(this IRevoConfiguration configuration,
            Action<AspNetConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<AspNetConfigurationSection>();
            section.IsActive = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    var hangfireConfigurationSection = configuration.GetSection<HangfireConfigurationSection>();
                    c.LoadModule(new CorePlatformModule(hangfireConfigurationSection));
                    c.LoadModule(new SecurityModule());
                    c.LoadModule(new NullAspNetIdentityModule());
                    c.LoadModule(new GlobalizationModule());
                    c.LoadModule(new TemplatesModule());
                }
            });

            return configuration;
        }
    }
}
