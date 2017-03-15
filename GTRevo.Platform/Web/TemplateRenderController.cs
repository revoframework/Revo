using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GTRevo.Platform.IO.Stache;
using Ninject;

namespace GTRevo.Platform.Web
{
    public abstract class TemplateRenderController : Controller
    {
        private string templateResourcePathRoot;

        public TemplateRenderController(string templateResourcePathRoot)
        {
            this.templateResourcePathRoot = templateResourcePathRoot;

            if (!this.templateResourcePathRoot.EndsWith("/"))
            {
                this.templateResourcePathRoot += "/";
            }
        }

        [Inject]
        public StacheRenderer StacheRenderer { get; set; }
        
        protected override void HandleUnknownAction(string actionName)
        {
            RenderTemplate(actionName).ExecuteResult(ControllerContext);
        }

        protected ActionResult RenderTemplate(string templateName)
        {
            //sanitize
            //TODO: perhaps leave this up to the resource provider?
            string sane = templateName.Replace('\\', '/');
            IEnumerable<string> templateNameParts = sane.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            templateNameParts = templateNameParts.Where(x => x != "." && x != "..");
            sane = string.Join("/", templateNameParts);

            if (sane.EndsWith(".html"))
            {
                sane = sane.Substring(0, sane.Length - ".html".Length) + ".cshtml";
            }
            
            return View(templateResourcePathRoot + sane);
        }
    }
}
