using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Core.Lifecycle;
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
