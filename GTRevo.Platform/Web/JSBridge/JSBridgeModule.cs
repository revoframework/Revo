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
