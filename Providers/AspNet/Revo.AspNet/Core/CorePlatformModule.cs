using System.Web.Hosting;
using Ninject.Modules;
using Revo.AspNet.Core.Lifecycle;
using Revo.AspNet.IO.Resources;
using Revo.AspNet.IO.Stache;
using Revo.AspNet.Web.VirtualPath;
using Revo.Core.Core;
using Revo.Core.IO.Resources;
using Revo.Core.Lifecycle;
using Revo.Core.Types;
using Revo.Hangfire;

namespace Revo.AspNet.Core
{
    [AutoLoadModule(false)]
    public class CorePlatformModule : NinjectModule
    {
        private readonly HangfireConfigurationSection hangfireConfigurationSection;

        public CorePlatformModule(HangfireConfigurationSection hangfireConfigurationSection)
        {
            this.hangfireConfigurationSection = hangfireConfigurationSection;
        }

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
            
            Bind<IConfiguration>()
                .ToMethod(ctx => LocalConfiguration.Current)
                .InTransientScope();

            Bind<IActorContext>()
                .To<UserActorContext>()
                .InTaskScope();
            
            Bind<IServiceLocator>()
                .To<NinjectServiceLocator>()
                .InSingletonScope();

            if (hangfireConfigurationSection.IsActive)
            {
                Bind<IOwinConfigurator>()
                    .To<HangfireOwinConfigurator>()
                    .InSingletonScope()
                    .WithConstructorArgument("jobStorage", ctx => hangfireConfigurationSection.JobStorage);
            }
        }
    }
}
