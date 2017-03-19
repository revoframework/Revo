using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Ninject.Modules;

namespace GTRevo.Platform.Web.JSBridge
{
    public class JSBridgeModule : NinjectModule
    {
        public override void Load()
        {
            Bind<JsonMessageExportCache>()
                .ToSelf()
                .InSingletonScope();

            Bind<JSServiceWrapperCache>()
                .ToSelf()
                .InSingletonScope();

            Bind<IApiExplorer>()
                .To<ApiExplorer>()
                .InSingletonScope();
        }
    }
}
