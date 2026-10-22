using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Revo.Core.Commands;
using Revo.Core.Configuration;
using Revo.Core.Lifecycle;
using Revo.Core.Services;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Sagas;
using Revo.Infrastructure.DataAccess;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Domain;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Upgrades;
using Revo.Infrastructure.Jobs.InMemory;
using Revo.Infrastructure.Repositories;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Security.Commands;
using Revo.Infrastructure.Tenancy;
using Revo.Infrastructure.Validation;

namespace Revo.Infrastructure;

public static class InfrastructureConfigurationExtensions
{
    public static IRevoConfiguration ConfigureInfrastructure(this IRevoConfiguration configuration,
        Action<InfrastructureConfigurationSection> action = null)
    {
        configuration.ConfigureCore();
        configuration.UseInMemoryJobs();

        var section = configuration.GetOrAddSection<InfrastructureConfigurationSection>(cfg =>
        {
            var s = new InfrastructureConfigurationSection();
            cfg.ConfigureServices(services =>
            {
                services.AddSingleton<IDatabaseInitializerDiscovery>(sp => new DatabaseInitializerDiscovery(sp));

                services.TryAddSingleton<IAsyncEventPipelineConfiguration>(s.AsyncEventPipeline);

                RegisterDataAccess(services);
                RegisterDatabaseMigrations(services, s.DatabaseMigrations);
                RegisterRepositories(services);
                RegisterTenancy(services, s.Tenancy);
                RegisterSecurity(services);
                RegisterValidation(services);

                if (s.EnableSagas)
                {
                    RegisterSagas(services);
                }
            });
            cfg.AddAfterConfigureServicesAction(services =>
            {
                if (s.AutoDiscoverDatabaseInitializers)
                {
                    DatabaseInitializerDiscovery.RegisterDiscoveredInitializers(cfg, services);
                }
            });
            return s;
        });

        action?.Invoke(section);

        return configuration;
    }

    private static void RegisterDataAccess(IServiceCollection services)
    {
        services.AddSingleton<DatabaseInitializerLoader>();
        services.AddTransient<IDatabaseInitializerLoader>(sp => sp.GetRequiredService<DatabaseInitializerLoader>());
        services.AddHostedService(sp => sp.GetRequiredService<DatabaseInitializerLoader>());
        services.AddSingleton<IDatabaseInitializerSorter, DatabaseInitializerDependencySorter>();
    }

    private static void RegisterDatabaseMigrations(IServiceCollection services, DatabaseMigrationsConfiguration configuration)
    {
        services.AddSingleton<IDatabaseMigrationsConfiguration>(configuration);
        services.AddSingleton<IDatabaseMigrationExecutionOptions>(configuration);
        services.AddSingleton<IDatabaseMigrationRegistry, DatabaseMigrationRegistry>();
        services.AddSingleton<IDatabaseMigrationSelector, DatabaseMigrationSelector>();
        services.AddSingleton<IDatabaseMigrationSelectorOptions>(configuration.MigrationSelectorOptions);
        services.AddScoped<IDatabaseMigrationExecutor, DatabaseMigrationExecutor>();
        services.AddScopedHostedLifecycleStartingService<DatabaseMigrationExecutionHook>();
        services.AddSingleton<IDatabaseMigrationDiscovery, FileDatabaseMigrationDiscovery>();
        services.AddSingleton<IDatabaseMigrationDiscovery, ResourceDatabaseMigrationDiscovery>();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.TryAddSingleton<IEventStreamSequenceNumbersUpgrade, EventStreamSequenceNumbersUpgrade>();
        services.TryAddSingleton<IEventStreamUpgrader, EventStreamUpgrader>();

        services.AddSingleton<EntityTypeManager>();
        services.AddSingleton<IEntityTypeManager>(sp => sp.GetRequiredService<EntityTypeManager>());
        services.AddScopedHostedService<EntityTypeManagerStartupTask>();

        services.AddScoped<IRepository, Repository>();
        services.AddTransient<IRepositoryFactory, RepositoryFactory>();
        services.AddSingleton<IEntityFactory, EntityFactory>();
        services.AddSingleton<IEventSourcedAggregateFactory, EventSourcedAggregateFactory>();
    }

    private static void RegisterTenancy(IServiceCollection services, TenancyConfiguration configuration)
    {
        services.AddScoped<ITenantContext, DefaultTenantContext>();

        if (configuration.EnableTenantRepositoryFilter)
        {
            services.AddTransient<IRepositoryFilter>(sp =>
                new TenantRepositoryFilter(sp.GetRequiredService<IServiceFactory<ITenantContext>>())
                {
                    NullTenantCanAccessOtherTenantsData = configuration.NullTenantCanAccessOtherTenantsData
                });
        }

        if (configuration.UseNullTenantContextResolver)
        {
            services.AddSingleton<ITenantContextResolver, NullTenantContextResolver>();
        }

        if (configuration.UseDefaultTenantProvider)
        {
            services.AddTransient<ITenantProvider, DefaultTenantProvider>();
        }

        services.AddTransient<ICommandBusMiddleware<ICommandBase>, TenantContextCommandBusMiddleware>();
    }

    private static void RegisterSecurity(IServiceCollection services)
    {
        services.AddScoped<IEntityQueryAuthorizer, EntityQueryAuthorizer>();
        services.AddScoped<IEntityQueryFilterFactory, EntityQueryFilterFactory>();
        services.AddSingleton<CommandPermissionCache>();
        services.AddTransient<ICommandPermissionCache>(sp => sp.GetRequiredService<CommandPermissionCache>());
        services.AddHostedService(sp => sp.GetRequiredService<CommandPermissionCache>());
        services.AddTransient<IPreCommandFilter<ICommandBase>, CommandPermissionAuthorizer>();
    }

    private static void RegisterValidation(IServiceCollection services)
    {
        services.AddSingleton<IPreCommandFilter<ICommandBase>, CommandAttributeValidationFilter>();
    }

    private static void RegisterSagas(IServiceCollection services)
    {
        services.AddSingleton<SagaConventionConfigurationCache>();
        services.AddSingleton<ISagaConventionConfigurationCache>(sp => sp.GetRequiredService<SagaConventionConfigurationCache>());

        services.AddScoped<ISagaLocator, KeySagaLocator>();
        services.AddSingleton<ISagaRegistry, SagaRegistry>();
        services.AddSingleton<ISagaConfigurator, ConventionSagaConfigurator>();
        services.AddScopedHostedService<SagaConfigurationLoader>();
        services.AddTransient<ISagaRepository, SagaRepository>();
        services.AddTransient<ISagaEventDispatcher, SagaEventDispatcher>();

        services.BindAsyncEventListener<SagaEventListener, SagaEventListener.SagaEventSequencer>();
    }
}
