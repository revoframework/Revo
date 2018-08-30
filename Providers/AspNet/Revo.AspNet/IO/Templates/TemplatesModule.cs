using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.AspNet.IO.Templates
{
    [AutoLoadModule(false)]
    public class TemplatesModule : NinjectModule
    {
        public override void Load()
        {
            Bind<RazorEngineTemplates, IApplicationStartListener>()
                .To<RazorEngineTemplates>()
                .InSingletonScope();

            Bind<RazorEngineTemplateManager>()
                .ToSelf()
                .InTransientScope();
        }
    }
}
