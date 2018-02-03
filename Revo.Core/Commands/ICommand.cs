namespace Revo.Core.Commands
{
    public interface ICommandBase
    {
    }

    public interface ICommand : ICommandBase
    {
    }

    public interface ICommand<out T> : ICommandBase
    {
    }
}
