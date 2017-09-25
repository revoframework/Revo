using GTRevo.Core.Commands;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Security.Commands;
using GTRevo.Platform.Core;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Security
{
    public class SecurityInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepositoryFilter>()
                .To<AuthorizingCrudRepositoryFilter>()
                .InTransientScope();

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
