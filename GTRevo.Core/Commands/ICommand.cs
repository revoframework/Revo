using MediatR;

namespace GTRevo.Commands
{
    public interface ICommandBase
    {
    }

    public interface ICommand : ICommandBase, IRequest
    {
    }

    public interface ICommand<out T> : ICommandBase, IRequest<T>
    {
    }
}
