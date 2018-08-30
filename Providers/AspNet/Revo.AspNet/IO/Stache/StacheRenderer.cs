using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Mvc;
using Knoema.Localization;

namespace Revo.AspNet.IO.Stache
{
    /// <summary>
    /// A fake Stache template renderer, only replacing some of the tokens (enough for now).
    /// </summary>
    public class StacheRenderer
    {
        public StacheRenderer()
        {
        }

        public VirtualPathProvider VirtualPathProvider { get; set; }
        public UrlHelper UrlHelper { get; set; }

        public string RenderResourceFile(string resourcePath)
        {
            string text;
            VirtualFile file = VirtualPathProvider.GetFile(resourcePath);
            using (Stream stream = file.Open())
            using (StreamReader reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            Regex regex = new Regex("\\(=makeFileUrl\\(\"(.+?)\"\\)\\)");
            text = regex.Replace(text, match =>
                 UrlHelper.Content(match.Groups[1].Value));

            regex = new Regex("\\(=makeurl\\(\"(.*?)\"\\)\\)");
            text = regex.Replace(text, match =>
                 UrlHelper.Content(match.Groups[1].Value.Length > 0 ? match.Groups[1].Value : "/"));

            regex = new Regex("\\(@(.*?)\\)");
            text = regex.Replace(text, match =>
                 LocalizationManager.Instance.Translate(resourcePath /*TODO*/, match.Groups[1].Value));

            return text;
        }
    }
}
