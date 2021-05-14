namespace Revo.Core.Lifecycle
{
    /// <summary>
    /// Any service that needs to be notified that the application is stopping.
    /// </summary>
    public interface IApplicationStoppingListener
    {
        /// <summary>
        /// Invoked by the framework when it receives the signal to stop.
        /// </summary>
        void OnApplicationStopping();
    }
}
