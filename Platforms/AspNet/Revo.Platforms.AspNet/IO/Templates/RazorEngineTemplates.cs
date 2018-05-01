using RazorEngine.Configuration;
using RazorEngine.Templating;
using Revo.Core.Lifecycle;

namespace Revo.Platforms.AspNet.IO.Templates
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
