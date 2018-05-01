using Ninject.Modules;
using Revo.Core.Lifecycle;

namespace Revo.Platforms.AspNet.IO.Templates
{
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
