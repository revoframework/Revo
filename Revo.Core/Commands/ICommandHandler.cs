using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    /// <summary>Handles commands of specified type.</summary>
    /// <typeparam name="T">Handled command type.</typeparam>
    public interface ICommandHandler<in T>
         where T : class, ICommand
    {
        Task HandleAsync(T command, CancellationToken cancellationToken);
    }

    /// <summary>Handles returning commands of specified type.</summary>
    /// <typeparam name="TQuery">Handled command type.</typeparam>
    /// <typeparam name="TResult">Command result type.</typeparam>
    public interface ICommandHandler<in TQuery, TResult>
        where TQuery : class, ICommand<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
