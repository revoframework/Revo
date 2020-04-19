namespace Revo.Core.Commands
{
    /// <summary>Handles queries commands of specified type.</summary>
    /// <typeparam name="TQuery">Handled query type.</typeparam>
    /// <typeparam name="TResult">Query result type.</typeparam>
    public interface IQueryHandler<in TQuery, TResult> : ICommandHandler<TQuery, TResult>
        where TQuery : class, IQuery<TResult>
    {
    }
}
