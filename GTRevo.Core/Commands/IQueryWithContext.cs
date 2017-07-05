namespace GTRevo.Core.Commands
{
    public interface IQueryWithContext<out T> : ICommandWithContext<T>, IQuery<T>
    {
    }
}
