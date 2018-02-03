using System.Net.Http;
using System.Text;
using System.Web.Http;
using Ninject;
using Revo.Platforms.AspNet.IO.Stache;

namespace Revo.Platforms.AspNet.Web
{
    public class ResourceApiController : ApiController
    {
        [Inject]
        public StacheRenderer StacheRenderer { get; set; }

        protected HttpResponseMessage CreateJsonStacheResponse(string jsonResourcePath)
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    StacheRenderer.RenderResourceFile(jsonResourcePath),
                    Encoding.UTF8, "application/json")
            };
        }
    }
}
