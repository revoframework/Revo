using Owin;

namespace Revo.Platforms.AspNet.Core.Lifecycle
{
    public interface IOwinConfigurator
    {
        void ConfigureApp(IAppBuilder app);
    }
}
