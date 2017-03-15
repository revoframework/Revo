using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GTRevo.Platform.Web.JSBridge
{
    [RoutePrefix("jsbridge")]
    public class JSBridgeController : Controller
    {
        private readonly JSMessageExportCache jsMessageExportCache;

        public JSBridgeController(JSMessageExportCache jsMessageExportCache)
        {
            this.jsMessageExportCache = jsMessageExportCache;
        }

        [Route("bc.bridge.globalization.messages.json")]
        public ActionResult GetMessagesJson(string localeCode)
        {
            return Content(jsMessageExportCache
                .GetExportedMessages(localeCode),
                "application/json");
        }

        //[Route("bc.bridge.wrapperServices.module.js")]
        public ActionResult GetWrapperServicesModuleJS()
        {
            return Content("");
        }
    }
}
