using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;
using Revo.DataAccess.Entities;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.DataAccess.Migrations;
using Revo.EFCore.DataAccess.Model;
using Revo.EFCore.DataAccess.Query;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.EFCore.DataAccess.Configuration
{
    public static class EFCoreDataAccessConfigurationExtensions
    {
        public static IRevoConfiguration UseEFCoreDataAccess(this IRevoConfiguration configuration,
            Action<DbContextOptionsBuilder> configurer,
            bool? useAsPrimaryRepository = true,
            Action<EFCoreDataAccessConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetOrAddSection<EFCoreDataAccessConfigurationSection>(cfg =>
            {
                var section = new EFCoreDataAccessConfigurationSection();
                section.IsActive = true;
                section.UseAsPrimaryRepository = useAsPrimaryRepository ?? section.UseAsPrimaryRepository;
                section.Configurer = configurer ?? section.Configurer;
                
                cfg.ConfigureServices(services =>
                {
                    if (!section.IsActive)
                    {
                        return;
                    }

                    services.AddScoped<IEFCoreCrudRepository, EFCoreCrudRepository>();
                    services.AddScoped<IEFCoreReadRepository>(sp => sp.GetRequiredService<IEFCoreCrudRepository>());

                    if (section.UseAsPrimaryRepository)
                    {
                        services.AddScoped<ICrudRepository>(sp => sp.GetRequiredService<IEFCoreCrudRepository>());
                        services.AddScoped<IReadRepository>(sp => sp.GetRequiredService<IEFCoreCrudRepository>());
                    }

                    services.AddScoped<IRequestDbContextCache, RequestDbContextCache>();

                    services.AddScoped<ICrudRepositoryFactory<IEFCoreCrudRepository>, EFCoreCrudRepositoryFactory>();
                    services.AddScoped<ICrudRepositoryFactory<IEFCoreReadRepository>, EFCoreCrudRepositoryFactory>();
                    if (section.UseAsPrimaryRepository)
                    {
                        services.AddScoped<ICrudRepositoryFactory<ICrudRepository>, EFCoreCrudRepositoryFactory>();
                        services.AddScoped<ICrudRepositoryFactory<IReadRepository>, EFCoreCrudRepositoryFactory>();
                    }

                    services.AddScoped<IEFCoreDatabaseAccess, EFCoreDatabaseAccess>();
                    services.AddSingleton<ModelDefinitionDiscovery>();
                    services.AddSingleton<EntityTypeDiscovery>();
                    services.AddSingleton<IDbContextFactory, DbContextFactory>();
                    services.AddSingleton(section);

                    if (section.EnableCustomQueryProvider)
                    {
                        services.AddSingleton<IEFCoreConfigurer, CustomQueryProviderConfigurer>();
                    }

                    if (section.EnableCustomQueryTranslationPreprocessor)
                    {
                        services.AddSingleton<IEFCoreConfigurer, CustomQueryTranslationConfigurer>();
                    }

                    foreach (var conventionFunc in section.Conventions)
                    {
                        services.AddTransient<IEFCoreConvention>(sp => conventionFunc(sp));
                    }

                    if (section.Configurer != null)
                    {
                        services.AddSingleton<IEFCoreConfigurer>(new ActionConfigurer(section.Configurer));
                    }

                    if (section.EnableMigrationProvider)
                    {
                        services.AddScoped<IDatabaseMigrationProvider, EFCoreDatabaseMigrationProvider>();
                        services.AddSingleton<IMigrationScripterFactory, MigrationScripterFactory>();
                    }
                });
                
                return section;
            });
            
            advancedAction?.Invoke(section);

            return configuration;
        }
    }
}
