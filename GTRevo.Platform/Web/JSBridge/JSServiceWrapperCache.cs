using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Web.JSBridge
{
    public class JSServiceWrapperCache
    {
        private readonly JSServiceWrapperGenerator jsServiceWrapperGenerator;
        private string jsServiceWrapper;

        public JSServiceWrapperCache(JSServiceWrapperGenerator jsServiceWrapperGenerator)
        {
            this.jsServiceWrapperGenerator = jsServiceWrapperGenerator;
            Initialize();
        }

        public string GetJSServiceWrapper()
        {
            return jsServiceWrapper;
        }

        private void Initialize()
        {
            jsServiceWrapper = jsServiceWrapperGenerator.Generate();
        }
    }
}
