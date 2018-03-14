using System.Web.Http.Description;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.Globalization;
using Revo.Infrastructure.Tenancy;

namespace Revo.Extensions.AspNet.Interop.Globalization
{
    public class GlobalizationInteropModule : NinjectModule
    {
        public override void Load()
        {
            Bind<JsonMessageExportCache>()
                .To<JsonMessageExportCache>()
                .InTenantSingletonScope();
            
            Bind<IEventListener<MessageRepositoryReloadedEvent>>()
                .To<DbMessageCacheReloader>()
                .InRequestOrJobScope();

            Bind<IApiExplorer>()
                .To<ApiExplorer>()
                .InSingletonScope();
        }
    }
}
