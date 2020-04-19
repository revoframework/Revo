using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public interface ICommandBusPipeline
    {
        Task<object> ProcessAsync(ICommandBase command, CommandBusMiddlewareDelegate executionHandler,
            ICommandBus commandBus, CommandExecutionOptions executionOptions, CancellationToken cancellationToken);
    }
}