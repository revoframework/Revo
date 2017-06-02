using System.Web;
using System.Web.Hosting;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.Events;
using GTRevo.Platform.IO.Globalization;
using GTRevo.Platform.IO.Resources;
using GTRevo.Platform.IO.Stache;
using GTRevo.Platform.Security;
using GTRevo.Platform.Security.Identity;
using GTRevo.Platform.Transactions;
using GTRevo.Platform.Web.JSBridge;
using GTRevo.Platform.Web.VirtualPath;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
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

            Bind<LocaleManager>()
                .ToSelf()
                .InSingletonScope();

            Bind<EmbeddedResourceVirtualPathProvider>()
                .ToSelf()
                .InSingletonScope();

            Bind<IWebActivatorExHooks>()
                .To<EmbeddedResourceAppInitializer>()
                .InSingletonScope();

            Bind<IResourceManager, IWebActivatorExHooks>()
                .To<ResourceManager>()
                .InSingletonScope();

            Bind<LocaleLoader>()
                .ToSelf()
                .InSingletonScope();

            Bind<IMessageRepository>()
                .To<MessageRepository>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<LocalizationAppInitializer>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<LocaleLoader>()
                .InSingletonScope();

            Bind<IOwinConfigurator>()
                .To<SecurityAppInitializer>()
                .InSingletonScope();
            
            Bind<IApplicationStartListener>()
                .To<AutoMapperInitializer>()
                .InSingletonScope();

            Bind<IConfiguration>()
                .To<WebConfiguration>()
                .InSingletonScope();

            Bind<IClock>()
                .ToMethod(ctx => Clock.Current)
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
