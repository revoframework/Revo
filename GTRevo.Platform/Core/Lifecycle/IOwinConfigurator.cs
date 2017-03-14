using Owin;

namespace GTRevo.Platform.Core.Lifecycle
{
    public interface IOwinConfigurator
    {
        void ConfigureApp(IAppBuilder app);
    }
}
