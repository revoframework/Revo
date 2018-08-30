using Owin;

namespace Revo.AspNet.Core.Lifecycle
{
    public interface IOwinConfigurator
    {
        void ConfigureApp(IAppBuilder app);
    }
}
