namespace GTRevo.Core.Commands
{
    public interface IQuery<out T> : ICommand<T>
    {
    }
}
