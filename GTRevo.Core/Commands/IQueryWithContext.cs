namespace GTRevo.Commands
{
    public interface IQueryWithContext<out T> : ICommandWithContext<T>, IQuery<T>
    {
    }
}
