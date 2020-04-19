using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    /// <summary>A command bus that implements how commands and queries are processed.</summary>
    /// <remarks>
    /// Note there may be more command buses registered in the system (local, remote...), so you typically
    /// should use <see cref="ICommandGateway"/> instead when sending commands from user code.
    /// </remarks>
    public interface ICommandBus
    {
        Task SendAsync(ICommandBase command, CancellationToken cancellationToken = default(CancellationToken));
        Task SendAsync(ICommandBase command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken));
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command,
            CancellationToken cancellationToken = default(CancellationToken));
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken));
        
    }
}
