using System;
using System.Linq;
using Revo.Core.Configuration;
using Revo.EFCore.Events;
using Revo.EFCore.EventStores;
using Revo.EFCore.Projections;
using Revo.EFCore.Sagas;
using Revo.Infrastructure;

namespace Revo.EFCore.AspNetCoreOData.Configuration
{
    public static class EFCoreAspNetCoreODataConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureEFCoreAspNetCoreOData(this IRevoConfiguration configuration,
            Action<EFCoreAspNetCoreODataConfigurationSection> action = null)
        {
            var section = configuration.GetSection<EFCoreAspNetCoreODataConfigurationSection>();
            action?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (!c.Kernel.GetBindings(typeof(EFCoreAspNetCoreODataConfigurationSection)).Any())
                {
                    c.Kernel.Bind<EFCoreAspNetCoreODataConfigurationSection>()
                        .ToConstant(section);
                }
            });

            return configuration;
        }
    }
}
