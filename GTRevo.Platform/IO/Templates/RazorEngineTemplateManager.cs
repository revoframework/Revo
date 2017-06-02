using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.IO.Resources;
using RazorEngine.Templating;

namespace GTRevo.Platform.IO.Templates
{
    /// <summary>
    /// Stubbed from http://antaris.github.io/RazorEngine/TemplateManager.html
    /// </summary>
    public class RazorEngineTemplateManager : ITemplateManager
    {
        private readonly IResourceManager resourceManager;

        public RazorEngineTemplateManager(IResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
        }

        public ITemplateSource Resolve(ITemplateKey key)
        {
            string templatePath = key.Name;

            string templateText;
            using (Stream stream = resourceManager.CreateReadStream(templatePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                templateText = reader.ReadToEnd();
            }

            return new LoadedTemplateSource(templateText, templatePath);
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
        {
            // If you can have different templates with the same name depending on the 
            // context or the resolveType you need your own implementation here!
            // Otherwise you can just use NameOnlyTemplateKey.
            return new NameOnlyTemplateKey(name, resolveType, context);
            // template is specified by full path
            //return new FullPathTemplateKey(name, fullPath, resolveType, context);
        }

        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            // You can disable dynamic templates completely. 
            // This just means all convenience methods (Compile and RunCompile) with
            // a TemplateSource will no longer work (they are not really needed anyway).
            throw new NotImplementedException("dynamic templates are not supported!");
        }
    }
}
