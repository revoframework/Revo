namespace Revo.Core.Commands
{
    public interface IQueryHandler<TQuery, TResult> : ICommandHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
    }
}
