namespace Revo.Core.Commands
{
    public interface IQuery<out T> : ICommand<T>
    {
    }
}
