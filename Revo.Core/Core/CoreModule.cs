using System;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;
using Revo.Core.Commands;
using Revo.Core.Configuration;
using Revo.Core.Events;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Revo.Core.Transactions;
using Revo.Core.Types;

namespace Revo.Core.Core
{
    [AutoLoadModule(false)]
    public class CoreModule : NinjectModule
    {
        private readonly CoreConfigurationSection coreConfigurationSection;

        public CoreModule(CoreConfigurationSection coreConfigurationSection)
        {
            this.coreConfigurationSection = coreConfigurationSection;
        }

        public override void Load()
        {
            Kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();

            Bind<IClock>()
                .ToMethod(ctx => Clock.Current)
                .InTransientScope();

            Bind<IEnvironment>()
                .To<Environment>()
                .InSingletonScope()
                .WithPropertyValue(nameof(Environment.IsDevelopmentOverride), coreConfigurationSection.IsDevelopmentEnvironment);

            Bind<IEventBus>()
                .To<EventBus>()
                .InTaskScope();

            Bind<IUnitOfWorkFactory>()
                .To<UnitOfWorkFactory>()
                .InTaskScope();

            Bind<IUnitOfWork>()
                .ToMethod(ctx => ctx.ContextPreservingGet<ICommandContext>().UnitOfWork
                                 ?? throw new InvalidOperationException("Trying to resolve IUnitOfWork when there is not one active in current command context"))
                .InTransientScope();

            Bind<IPublishEventBufferFactory>()
                .To<PublishEventBufferFactory>()
                .InTaskScope();

            Bind<IPublishEventBuffer>()
                .ToMethod(ctx => ctx.ContextPreservingGet<IUnitOfWork>().EventBuffer)
                .InTransientScope();

            Rebind<ITypeExplorer>()
                .To<TypeExplorer>()
                .InSingletonScope();

            Rebind<ITypeIndexer>()
                .To<TypeIndexer>()
                .InSingletonScope();

            Rebind<IVersionedTypeRegistry>()
                .To<VersionedTypeRegistry>()
                .InSingletonScope();

            Bind<IPermissionTypeRegistry>()
                .To<PermissionTypeRegistry>()
                .InSingletonScope();

            Bind<IPermissionTypeIndexer, IApplicationStartedListener>()
                .To<PermissionTypeIndexer>()
                .InSingletonScope();

            Bind<IPermissionAuthorizationMatcher>()
                .To<PermissionAuthorizationMatcher>()
                .InTaskScope();
            
            Bind<IUserPermissionAuthorizer>()
                .To<UserPermissionAuthorizer>()
                .InTaskScope();
        }
    }
}
