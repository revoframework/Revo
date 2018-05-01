using Hangfire;
using Hangfire.MemoryStorage;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Revo.Domain.Entities;
using Revo.Examples.HelloAspNet.Bootstrap.Domain;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Projections;
using Revo.Examples.HelloAspNet.Bootstrap.Services;
using Revo.Infrastructure.EF6.Projections;
using Revo.Platforms.AspNet.Security.Identity;

namespace Revo.Examples.HelloAspNet.Bootstrap
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEF6EntityEventProjector<Todo>>()
                .To<TodoReadModelProjector>()
                .InRequestOrJobScope();
            
            Bind<IAppUserStore>()
                .To<AppUserStore>()
                .InTransientScope();

            Bind<IRolePermissionResolver>()
                .To<RolePermissionResolver>()
                .InTransientScope();

            Bind<IEntityTypeManager, IApplicationStartListener>()
                .To<EntityTypeManager>()
                .InRequestOrJobScope();

            GlobalConfiguration.Configuration.UseMemoryStorage();
        }
    }
}