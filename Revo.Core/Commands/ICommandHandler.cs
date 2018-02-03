using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public interface ICommandHandler<in T>
         where T : ICommand
    {
        Task HandleAsync(T command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TQuery, TResult>
        where TQuery : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
