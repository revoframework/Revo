namespace Revo.Platforms.AspNet.Core.Lifecycle
{
    public interface IWebActivatorExHooks
    {
        void OnPostApplicationStart();
        void OnApplicationShutdown();
    }
}
