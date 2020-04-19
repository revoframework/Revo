namespace Revo.Core.Commands
{
    /// <summary>
    /// Query object. Classes implementing this interface define a query that the framework can process.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    public interface IQuery<out T> : ICommand<T>
    {
    }
    
}
