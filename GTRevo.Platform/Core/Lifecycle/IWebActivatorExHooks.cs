namespace GTRevo.Platform.Core.Lifecycle
{
    public interface IWebActivatorExHooks
    {
        void OnPreApplicationStart();
        void OnPostApplicationStart();
        void OnApplicationShutdown();
    }
}
