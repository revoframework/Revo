using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Revo.Core.Services;
using Revo.Core.Transactions;
using Revo.Core.Types;
using Environment = Revo.Core.Core.Environment;

namespace Revo.Core.Configuration;

public static class CoreConfigurationExtensions
{
    public static IRevoConfiguration ConfigureCore(this IRevoConfiguration configuration,
        Action<CoreConfigurationSection> advancedAction = null)
    {
        var section = configuration.GetOrAddSection<CoreConfigurationSection>(cfg =>
        {
            var coreSection = new CoreConfigurationSection();
            cfg.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(cfg);
                serviceCollection.AddSingleton(coreSection);
                serviceCollection.AddSingleton<ITypesConfiguration>(serviceProvider =>
                    serviceProvider.GetRequiredService<CoreConfigurationSection>());
                serviceCollection.AddSingleton(new VariantServiceTypeIndex(serviceCollection));
                serviceCollection.AddTransient<IVariantServiceResolver, VariantServiceResolver>();
                serviceCollection.AddTransient(typeof(IServiceFactory<>), typeof(ContainerServiceFactory<>));
                serviceCollection.AddTransient<IClock>(_ => Clock.Current);
                serviceCollection.AddSingleton<IEnvironment, Environment>();
                serviceCollection.AddScoped<IEventBus, EventBus>();
                serviceCollection.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
                // transient, so that it always reflects the unit of work active at the time of the resolution
                serviceCollection.AddTransient<IUnitOfWork>(serviceProvider =>
                    serviceProvider.GetRequiredService<ICommandContext>().UnitOfWork
                    ?? throw new InvalidOperationException(
                        "Trying to resolve IUnitOfWork when there is not one active in current command context"));
                serviceCollection.AddScoped<IPublishEventBufferFactory, PublishEventBufferFactory>();
                serviceCollection.AddTransient<IPublishEventBuffer>(serviceProvider =>
                    serviceProvider.GetRequiredService<IUnitOfWork>().EventBuffer);
                serviceCollection.TryAddSingleton<IRequestLifetime, NullRequestLifetime>();
                serviceCollection.AddSingleton<ITypeExplorer, TypeExplorer>();
                serviceCollection.AddSingleton<ITypeIndexer, TypeIndexer>();
                serviceCollection.AddSingleton<IVersionedTypeRegistry, VersionedTypeRegistry>();
                serviceCollection.AddSingleton<IPermissionTypeRegistry, PermissionTypeRegistry>();

                serviceCollection.AddSingleton<PermissionTypeIndexer>();
                serviceCollection.AddTransient<IPermissionTypeIndexer>(serviceProvider =>
                    serviceProvider.GetRequiredService<PermissionTypeIndexer>());
                serviceCollection.AddHostedService(sp => sp.GetRequiredService<PermissionTypeIndexer>());
            });
            return coreSection;
        });

        advancedAction?.Invoke(section);

        return configuration;
    }
}
