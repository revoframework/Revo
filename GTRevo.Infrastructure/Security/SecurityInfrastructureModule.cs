using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.Security.Commands;
using GTRevo.Platform.Commands;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.Events;
using GTRevo.Platform.Transactions;
using MediatR;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Security
{
    public class SecurityInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Rebind<ICrudRepository>()
                .To<CrudRepository>()
                .WhenInjectedInto<AuthorizingCrudRepository>()
                .InTransientScope();

            Bind<ICrudRepository, IAuthorizingCrudRepository>()
                .To<AuthorizingCrudRepository>()
                .InRequestOrJobScope();

            Bind<IEntityQueryAuthorizer>()
                .To<EntityQueryAuthorizer>()
                .InRequestOrJobScope();

            Bind<IEntityQueryFilterFactory>()
                .To<EntityQueryFilterFactory>()
                .InRequestOrJobScope();

            Bind<CommandPermissionCache, IApplicationStartListener>()
                .To<CommandPermissionCache>()
                .InSingletonScope();

            Bind<ICommandFilter<ICommandBase>>()
                .To<CommandPermissionAuthorizer>()
                .InTransientScope();
        }
    }
}
