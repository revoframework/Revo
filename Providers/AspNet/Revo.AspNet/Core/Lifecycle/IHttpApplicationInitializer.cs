using System.Web;

namespace Revo.AspNet.Core.Lifecycle
{
    public interface IHttpApplicationInitializer
    {
        void OnApplicationStart(HttpApplication application);
    }
}
