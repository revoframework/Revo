namespace Revo.Core.Lifecycle
{
    /// <summary>
    /// Any service that needs to be notified that the application has started.
    /// </summary>
    public interface IApplicationStartedListener
    {
        /// <summary>
        /// Invoked by the framework right after starting the application.
        /// At this point, the framework will already be fully initialized and the application might already
        /// be processing requests. This is usually a good moment to hook other non-essential application
        /// runtime parts (e.g. connect to integration middlewares).
        /// </summary>
        void OnApplicationStarted();
    }
}
