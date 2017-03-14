namespace GTRevo.Platform.Commands
{
    public interface IAsyncQueryHandler<TQuery, TResult> : IAsyncCommandHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
    }
}
