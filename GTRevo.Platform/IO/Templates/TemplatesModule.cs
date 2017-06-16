using GTRevo.Core.Lifecycle;
using Ninject.Modules;

namespace GTRevo.Platform.IO.Templates
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
