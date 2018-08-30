using Hangfire;
using Hangfire.MemoryStorage;
using Ninject.Modules;
using Revo.AspNet.Core.Lifecycle;
using Revo.Core.Core;
using Revo.EF6.Projections;
using Revo.Examples.HelloAspNet.Bootstrap.Domain;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Projections;

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