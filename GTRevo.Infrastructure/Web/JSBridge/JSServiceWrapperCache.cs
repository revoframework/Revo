namespace GTRevo.Infrastructure.Web.JSBridge
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
