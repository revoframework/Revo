namespace Revo.Core.Commands
{
    public interface ICommandWithContext : ICommand, IHasContext
    {
    }

    public interface ICommandWithContext<out T> : ICommand<T>, IHasContext
    {
    }
}
