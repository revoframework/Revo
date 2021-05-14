namespace Revo.Core.Lifecycle
{
    /// <summary>
    /// Any service that needs to be notified that the application is about to be started.
    /// </summary>
    public interface IApplicationStartingListener
    {
        /// <summary>
        /// Invoked by the framework before starting the application.
        /// At this point, the application is not running yet (e.g. not processing any requests),
        /// but the framework has already been fully configured by all of <see cref="IApplicationConfigurer"/>.
        /// </summary>
        void OnApplicationStarting();
    }
}