using System.Web.Hosting;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Platforms.AspNet.Core.Lifecycle;
using Revo.Platforms.AspNet.IO.Resources;
using Revo.Platforms.AspNet.IO.Stache;
using Revo.Platforms.AspNet.Security;
using Revo.Platforms.AspNet.Web.VirtualPath;

namespace Revo.Platforms.AspNet.Core
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
            
            Bind<AutoMapperDefinitionDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<IOwinConfigurator>()
                .To<HangfireOwinConfigurator>()
                .InSingletonScope();

            Bind<IServiceLocator>()
                .To<NinjectServiceLocator>()
                .InSingletonScope();
        }
    }
}
