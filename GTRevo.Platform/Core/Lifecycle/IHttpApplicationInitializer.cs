using System.Web;

namespace GTRevo.Platform.Core.Lifecycle
{
    public interface IHttpApplicationInitializer
    {
        void OnApplicationStart(HttpApplication application);
    }
}
