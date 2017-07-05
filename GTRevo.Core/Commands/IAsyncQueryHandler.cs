namespace GTRevo.Core.Commands
{
    public interface IAsyncQueryHandler<TQuery, TResult> : IAsyncCommandHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
    }
}
