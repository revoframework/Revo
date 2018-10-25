using System;
using System.Linq;
using Ninject;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.IO;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Revo.Core.Transactions;
using Revo.Core.Types;

namespace Revo.Core.Core
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IClock>()
                .ToMethod(ctx => Clock.Current)
                .InTransientScope();

            Bind<IApplicationConfigurer>()
                .To<CommandHandlerDiscovery>()
                .InSingletonScope();

            Bind<IEventBus>()
                .To<EventBus>()
                .InTaskScope();

            Bind<ICommandContext, CommandContextStack>()
                .To<CommandContextStack>()
                .InTaskScope();

            Bind<IUnitOfWorkFactory>()
                .To<UnitOfWorkFactory>()
                .InTaskScope();

            Bind<IUnitOfWork>()
                .ToMethod(ctx => ctx.ContextPreservingGet<ICommandContext>().UnitOfWork ?? throw new InvalidOperationException("Trying to resolve IUnitOfWork when there is no instance active in current command context"))
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

            Bind<PermissionTypeIndexer, IApplicationStartListener>()
                .To<PermissionTypeIndexer>()
                .InSingletonScope();

            Bind<IPermissionAuthorizationMatcher>()
                .To<PermissionAuthorizationMatcher>()
                .InTaskScope();

            Bind<IPermissionCache>()
                .To<PermissionCache>()
                .InSingletonScope();

            Bind<IUserPermissionAuthorizer>()
                .To<UserPermissionAuthorizer>()
                .InTaskScope();

            Bind<IAutoMapperProfileDiscovery>()
                .To<AutoMapperProfileDiscovery>()
                .InSingletonScope();

            Bind<IApplicationConfigurer>()
                .To<AutoMapperInitializer>()
                .InSingletonScope();
        }
    }
}
