using System.Web.Hosting;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.IO;
using Revo.Core.IO.Resources;
using Revo.Core.Lifecycle;
using Revo.Core.Types;
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
            Rebind<ITypeExplorer>()
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

            Bind<IResourceManager, IAspNetResourceManager, IWebActivatorExHooks>()
                .To<AspNetResourceManager>()
                .InSingletonScope();
            
            Bind<IApplicationConfigurer>()
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
