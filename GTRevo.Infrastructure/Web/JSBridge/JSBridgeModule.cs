using System.Web.Http.Description;
using GTRevo.Core.Core;
using GTRevo.Infrastructure.Globalization.Messages.Database;
using GTRevo.Infrastructure.Tenancy;
using MediatR;
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
            
            Bind<IAsyncNotificationHandler<DbMessageCacheReloadedEvent>>()
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
