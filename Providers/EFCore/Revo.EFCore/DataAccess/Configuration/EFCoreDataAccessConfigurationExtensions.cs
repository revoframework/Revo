using System;
using Microsoft.EntityFrameworkCore;
using Revo.Core.Configuration;
using Revo.EFCore.DataAccess.Conventions;

namespace Revo.EFCore.DataAccess.Configuration
{
    public static class EFCoreDataAccessConfigurationExtensions
    {
        public static IRevoConfiguration UseEFCoreDataAccess(this IRevoConfiguration configuration,
            Action<DbContextOptionsBuilder> configurer,
            bool? useAsPrimaryRepository = true,
            Action<EFCoreDataAccessConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EFCoreDataAccessConfigurationSection>();
            section.IsActive = true;
            section.UseAsPrimaryRepository = useAsPrimaryRepository ?? section.UseAsPrimaryRepository;
            section.Configurer = configurer ?? section.Configurer;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.Kernel.Bind<EFCoreDataAccessConfigurationSection>().ToConstant(section);

                    c.LoadModule(new EFCoreDataAccessModule(section.UseAsPrimaryRepository));

                    foreach (var conventionFunc in section.Conventions)
                    {
                        c.Kernel.Bind<IEFCoreConvention>().ToMethod(conventionFunc);
                    }

                    if (section.Configurer != null)
                    {
                        c.Kernel.Bind<IEFCoreConfigurer>().ToConstant(new ActionConfigurer(section.Configurer));
                    }
                }
            });

            return configuration;
        }
    }
}
