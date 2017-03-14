namespace GTRevo.Platform.Commands
{
    public interface IQuery<out T> : ICommand<T>
    {
    }
}
