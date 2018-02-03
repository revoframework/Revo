using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Revo.Platforms.AspNet.IO.Stache
{
    public class WebStacheRenderer : StacheRenderer
    {
        public WebStacheRenderer(HttpContext httpContext, VirtualPathProvider virtualPathProvider)
        {
            UrlHelper = new UrlHelper(httpContext.Request.RequestContext);
            VirtualPathProvider = virtualPathProvider;
        }
    }
}
