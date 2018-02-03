namespace Revo.Platforms.AspNet.Core.Lifecycle
{
    public interface IWebActivatorExHooks
    {
        void OnPreApplicationStart();
        void OnPostApplicationStart();
        void OnApplicationShutdown();
    }
}
