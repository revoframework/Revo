using System.Web.Http.Description;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Globalization;
using GTRevo.Infrastructure.Tenancy;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Web.JSBridge
{
    public class JSBridgeModule : NinjectModule
    {
        public override void Load()
        {
            Bind<JsonMessageExportCache>()
                .To<JsonMessageExportCache>()
                .InTenantSingletonScope();
            
            Bind<IEventListener<MessageRepositoryReloadedEvent>>()
                .To<DbMessageCacheReloader>()
                .InRequestOrJobScope();

            Bind<JSServiceWrapperCache>()
                .ToSelf()
                .InSingletonScope();

            Bind<IApiExplorer>()
                .To<ApiExplorer>()
                .InSingletonScope();
        }
    }
}
