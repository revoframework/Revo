namespace Revo.Core.Core
{
    /// <summary>
    /// Environment in which the application is run.
    /// The environment can specify certain parameters that can affect the way the application
    /// behaves (e.g. file paths, logging levels, applying database migrations, etc.).
    /// </summary>
    public interface IEnvironment
    {
        bool IsDevelopment { get; }
    }
}