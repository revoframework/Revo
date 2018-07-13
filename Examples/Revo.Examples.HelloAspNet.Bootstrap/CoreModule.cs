using Hangfire;
using Hangfire.MemoryStorage;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Revo.Domain.Entities;
using Revo.Examples.HelloAspNet.Bootstrap.Domain;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Projections;
using Revo.Infrastructure.EF6.Projections;
using Revo.Platforms.AspNet.Core.Lifecycle;
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

            Bind<IHttpApplicationInitializer>()
                .To<AppStartHttpApplicationInitializer>()
                .InSingletonScope();

            GlobalConfiguration.Configuration.UseMemoryStorage();
        }
    }
}