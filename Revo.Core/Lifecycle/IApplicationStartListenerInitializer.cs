namespace Revo.Core.Lifecycle
{
    public interface IApplicationStartListenerInitializer
    {
        void InitializeStarted();
        void DeinitializeStopping();
    }
}