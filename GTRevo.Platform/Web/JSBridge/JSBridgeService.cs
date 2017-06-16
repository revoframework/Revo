using System.Net.Http;
using System.Text;
using System.Web.Http;
using GTRevo.Platform.IO.Stache;

namespace GTRevo.Platform.Web.JSBridge
{
    [RoutePrefix("jsbridge")]
    public class JSBridgeService : ApiController
    {
        private readonly JsonMessageExportCache jsMessageExportCache;
        private readonly JSServiceWrapperCache jsServiceWrapperCache;
        private readonly StacheRenderer stacheRenderer;

        public JSBridgeService(JsonMessageExportCache jsMessageExportCache,
            JSServiceWrapperCache jsServiceWrapperCache,
            StacheRenderer stacheRenderer)
        {
            this.jsMessageExportCache = jsMessageExportCache;
            this.jsServiceWrapperCache = jsServiceWrapperCache;
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

        [Route("bc.bridge.wrapperServices.module.js")]
        public HttpResponseMessage GetWrapperServicesModuleJS()
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                       jsServiceWrapperCache.GetJSServiceWrapper(),
                       Encoding.UTF8, "application/javascript")
            };
        }

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
