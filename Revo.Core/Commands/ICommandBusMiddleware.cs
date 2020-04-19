using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    /// <summary>
    /// Executes an aspect of handling a command (e.g. committing unit of work, logging,
    /// exception handling...).
    /// </summary>
    /// <typeparam name="T">Command base type (contravariant).</typeparam>
    public interface ICommandBusMiddleware<in T>
        where T : class, ICommandBase
    {
        int Order { get; set; }

        Task<object> HandleAsync(T command, CommandExecutionOptions executionOptions,
            CommandBusMiddlewareDelegate next, CancellationToken cancellationToken);
    }
}