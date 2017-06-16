using GTRevo.Commands;
using GTRevo.Core.Lifecycle;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.Security.Commands;
using GTRevo.Platform.Core;
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
