using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Infrastructure.Security
{
    public class SecurityInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEntityQueryAuthorizer>()
                .To<EntityQueryAuthorizer>()
                .InTaskScope();

            Bind<IEntityQueryFilterFactory>()
                .To<EntityQueryFilterFactory>()
                .InTaskScope();

            Bind<ICommandPermissionCache, IApplicationStartedListener>()
                .To<CommandPermissionCache>()
                .InSingletonScope();

            Bind<IPreCommandFilter<ICommandBase>>()
                .To<CommandPermissionAuthorizer>()
                .InTransientScope();
        }
    }
}
