using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire;
using Hangfire.MemoryStorage;
using Ninject;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Core.Security;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.EF6.InMemory;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Revo.Domain.Entities;
using Revo.Examples.HelloAspNet.Bootstrap.Domain;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Projections;
using Revo.Examples.HelloAspNet.Bootstrap.Services;
using Revo.Infrastructure.EF6.Events.Async;
using Revo.Infrastructure.EF6.EventStore;
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