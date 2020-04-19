namespace Revo.Core.Commands
{
    /// <summary>
    /// Base interface for all commands and queries. Do not implement this interface, use <see cref="ICommand"/> or <see cref="IQuery{T}"/> instead.
    /// </summary>
    public interface ICommandBase
    {
    }

    /// <summary>A command object.</summary>
    public interface ICommand : ICommandBase
    {
    }

    /// <summary>A command object whose handler also returns a value when processed.</summary>
    /// <remarks>Using commands that return value is usually not recommended. With CQRS, commands should usually
    /// only modify state and not return anything, while queries should only return current state.</remarks>
    /// <typeparam name="T">Result type.</typeparam>
    /// <seealso cref="IQuery{T}"/>
    public interface ICommand<out T> : ICommandBase
    {
    }
}
