using System.IO;
using System.Web;
using System.Web.Hosting;
using Revo.AspNet.IO.Resources;

namespace Revo.AspNet.Web.VirtualPath
{
    class EmbeddedResourceVirtualFile : VirtualFile
    {
        readonly EmbeddedResource embedded;
        readonly EmbeddedResourceCacheControl cacheControl;

        public EmbeddedResourceVirtualFile(string virtualPath, EmbeddedResource embedded, EmbeddedResourceCacheControl cacheControl)
            : base(virtualPath)
        {
            this.embedded = embedded;
            this.cacheControl = cacheControl;
        }

        public override Stream Open()
        {
            if (cacheControl != null)
            {
                HttpContext.Current.Response.Cache.SetCacheability(cacheControl.Cacheability);
                HttpContext.Current.Response.Cache.AppendCacheExtension("max-age=" + cacheControl.MaxAge);
            }

            return embedded.GetStream();
        }
    }
}