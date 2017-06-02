using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Core.Lifecycle;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace GTRevo.Platform.IO.Templates
{
    public class RazorEngineTemplates : IApplicationStartListener
    {
        private readonly RazorEngineTemplateManager razorEngineTemplateManager;

        public RazorEngineTemplates(RazorEngineTemplateManager razorEngineTemplateManager)
        {
            this.razorEngineTemplateManager = razorEngineTemplateManager;
        }

        public void OnApplicationStarted()
        {
            TemplateServiceConfiguration config = new TemplateServiceConfiguration();
#if DEBUG
            config.Debug = true;
#endif
            config.TemplateManager = razorEngineTemplateManager;
            EngineService = RazorEngineService.Create(config);
        }

        public IRazorEngineService EngineService { get; private set; }
    }
}
