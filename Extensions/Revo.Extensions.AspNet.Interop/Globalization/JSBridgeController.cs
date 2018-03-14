using System.Net.Http;
using System.Text;
using System.Web.Http;
using Revo.Platforms.AspNet.IO.Stache;

namespace Revo.Extensions.AspNet.Interop.Globalization
{
    [RoutePrefix("jsbridge")]
    public class JSBridgeController : ApiController
    {
        private readonly JsonMessageExportCache jsMessageExportCache;
        private readonly StacheRenderer stacheRenderer;

        public JSBridgeController(JsonMessageExportCache jsMessageExportCache,
            StacheRenderer stacheRenderer)
        {
            this.jsMessageExportCache = jsMessageExportCache;
            this.stacheRenderer = stacheRenderer;
        }

        [Route("bc.bridge.globalization.messages.json")]
        public HttpResponseMessage GetMessagesJson(string localeCode)
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    jsMessageExportCache.GetExportedMessages(localeCode),
                    Encoding.UTF8, "application/json")
            };
        }
        
        [Route("bc.bridge.model-exporter-data.js")]
        public HttpResponseMessage GetModelExporterData()
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    stacheRenderer.RenderResourceFile("~/jsbridge/bc.bridge.model-exporter.data.json"),
                    Encoding.UTF8, "application/json")
            };
        }
    }
}
