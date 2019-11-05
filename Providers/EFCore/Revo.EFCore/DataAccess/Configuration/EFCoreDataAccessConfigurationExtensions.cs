using System;
using Microsoft.EntityFrameworkCore;
using Revo.Core.Configuration;
using Revo.EFCore.DataAccess.Migrations;

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
                    c.LoadModule(new EFCoreDataAccessModule(section));

                    if (section.EnableMigrationProvider)
                    {
                        c.LoadModule(new EFCoreMigrationsModule(section));
                    }
                }
            });

            return configuration;
        }
    }
}
