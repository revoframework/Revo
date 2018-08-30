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
            section.Configurer = configurer;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new EFCoreDataAccessModule(section.UseAsPrimaryRepository));

                    if (section.ConventionTypes != null)
                    {
                        foreach (var conventionType in section.ConventionTypes)
                        {
                            c.Kernel.Bind<IEFCoreConvention>().To(conventionType);
                        }
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
