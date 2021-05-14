namespace Revo.Core.Lifecycle
{
    /// <summary>
    /// Any service that configures the framework before starting the application.
    /// </summary>
    public interface IApplicationConfigurer
    {
        /// <summary>
        /// Invoked by the framework during the bootstrap configuration stage.
        /// At this point, the application is not running yet (e.g. not processing any requests)
        /// and the startup process has not begun.
        /// </summary>
        void Configure();
    }
}
