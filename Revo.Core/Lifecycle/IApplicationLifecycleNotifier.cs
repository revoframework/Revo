namespace Revo.Core.Lifecycle
{
    public interface IApplicationLifecycleNotifier
    {
        void NotifyStarting();
        void NotifyStarted();
        void NotifyStopping();
    }
}