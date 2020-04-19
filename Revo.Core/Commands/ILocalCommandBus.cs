namespace Revo.Core.Commands
{
    /// <summary>
    /// Local command bus that is implemented using <see cref="ICommandHandler{T}"/> handlers
    /// that are locally implemented and registered.
    /// </summary>
    public interface ILocalCommandBus : ICommandBus
    {
    }
}