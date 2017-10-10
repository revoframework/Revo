using System.Web.Hosting;
using GTRevo.Core;
using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.IO.Resources;
using GTRevo.Platform.IO.Stache;
using GTRevo.Platform.Security;
using GTRevo.Platform.Web.VirtualPath;
using Ninject.Modules;

namespace GTRevo.Platform.Core
{
    public class CorePlatformModule : NinjectModule
    {
        public override void Load()
        {
            /*Bind<HttpContext>()
                .ToMethod(ctx => HttpContext.Current)
                .InTransientScope();*/

            Bind<ITypeExplorer>()
                .To<AspNetTypeExplorer>()
                .InSingletonScope();

            Bind<VirtualPathProvider>()
                .ToMethod(ctx => HostingEnvironment.VirtualPathProvider)
                .InSingletonScope();

            Bind<StacheRenderer>()
                .To<WebStacheRenderer>()
                .InTransientScope();

            Bind<EmbeddedResourceVirtualPathProvider>()
                .ToSelf()
                .InSingletonScope();

            Bind<IWebActivatorExHooks>()
                .To<EmbeddedResourceAppInitializer>()
                .InSingletonScope();

            Bind<IResourceManager, IWebActivatorExHooks>()
                .To<ResourceManager>()
                .InSingletonScope();

            Bind<IOwinConfigurator>()
                .To<SecurityAppInitializer>()
                .InSingletonScope();
            
            Bind<IApplicationStartListener>()
                .To<AutoMapperInitializer>()
                .InSingletonScope();

            Bind<IConfiguration>()
                .ToMethod(ctx => LocalConfiguration.Current)
                .InTransientScope();

            Bind<IActorContext>()
                .To<UserActorContext>()
                .InRequestOrJobScope();

            Bind<IUnitOfWork>()
                 .To<UnitOfWork>()
                 .InRequestOrJobScope();

            Bind<IEventQueue>()
                .To<EventQueue>()
                .InRequestOrJobScope();

            Bind<AutoMapperDefinitionDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<IOwinConfigurator>()
                .To<HangfireOwinConfigurator>()
                .InSingletonScope();
        }
    }
}
