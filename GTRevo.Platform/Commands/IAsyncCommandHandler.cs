using MediatR;

namespace GTRevo.Platform.Commands
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
