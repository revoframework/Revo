using System.Web;

namespace Revo.Platforms.AspNet.Core.Lifecycle
{
    public interface IHttpApplicationInitializer
    {
        void OnApplicationStart(HttpApplication application);
    }
}
