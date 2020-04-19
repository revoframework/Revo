using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    /// <summary>Gateway entry-point for sending commands and queries and getting them processed.</summary>
    /// <remarks>
    /// Based on the routes registered for individual command (or query) types in the <see cref="ICommandRouter"/>,
    /// command gateway sends the commands to different command buses (e.g. local, remote...).
    /// </remarks>
    public interface ICommandGateway
    {
        /// <summary>
        /// Sends a command to registered command bus (e.g. local command bus using <see cref="ICommandHandler{T}"/>)
        /// and waits for its processing to finish.
        /// 
        /// Uses default command execution options.
        /// </summary>
        /// <param name="command">Query object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendAsync(ICommandBase command, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sends a command to registered command bus (e.g. local command bus using <see cref="ICommandHandler{T}"/>)
        /// and waits for its processing to finish.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="executionOptions">Custom command execution options, defaults to <see cref="CommandExecutionOptions.Default"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendAsync(ICommandBase command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Sends a query to registered command bus (e.g. local command bus using <see cref="IQueryHandler{TQuery,TResult}"/>)
        /// and waits for its processing to finish.
        /// 
        /// Uses default command execution options.
        /// </summary>
        /// <typeparam name="TResult">Query result type.</typeparam>
        /// <param name="command">Query object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Query result.</returns>
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sends a query to registered command bus (e.g. local command bus using <see cref="IQueryHandler{TQuery,TResult}"/>)
        /// and waits for its processing to finish.
        /// </summary>
        /// <typeparam name="TResult">Query result type.</typeparam>
        /// <param name="command">Query object.</param>
        /// <param name="executionOptions">Custom command execution options, defaults to <see cref="CommandExecutionOptions.Default"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Query result.</returns>
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}