using System;
using Revo.AspNet.Core;
using Revo.AspNet.IO.Templates;
using Revo.AspNet.Security;
using Revo.Core.Configuration;
using Revo.Hangfire;

namespace Revo.AspNet
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
                    c.LoadModule(new TemplatesModule());
                }
            });

            return configuration;
        }
    }
}
