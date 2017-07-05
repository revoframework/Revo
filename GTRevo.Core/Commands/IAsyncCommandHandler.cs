using MediatR;

namespace GTRevo.Core.Commands
{
    public interface IAsyncCommandHandler<T> : IAsyncRequestHandler<T>
         where T : ICommand
    {
    }

    public interface IAsyncCommandHandler<TQuery, TResult> : IAsyncRequestHandler<TQuery, TResult>
        where TQuery : ICommand<TResult>
    {
    }
}
