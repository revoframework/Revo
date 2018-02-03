using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Revo.Platforms.AspNet.Web
{
    public class CustomRazorViewEngine : RazorViewEngine
    { 
        public CustomRazorViewEngine()
        {
            OverrideTemplatePaths();
        }

        public CustomRazorViewEngine(IViewPageActivator viewPageActivator) : base(viewPageActivator)
        {
            OverrideTemplatePaths();
        }

        private void OverrideTemplatePaths()
        {
            List<string> paths = ViewLocationFormats.ToList();
            paths.Insert(0, "~/{0}");
            ViewLocationFormats = paths.ToArray();

            paths = PartialViewLocationFormats.ToList();
            paths.Insert(0, "~/{0}");
            PartialViewLocationFormats = paths.ToArray();
        }
    }
}
