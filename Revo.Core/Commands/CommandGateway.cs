using System;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public class CommandGateway(ICommandRouter commandRouter) : ICommandGateway
    {
        public Task SendAsync(ICommandBase command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandBus = commandRouter.FindRoute(command.GetType())
                             ?? throw new ArgumentException($"No route to a command bus found for command type {command.GetType()}");
            return commandBus.SendAsync(command, executionOptions, cancellationToken);
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(command, CommandExecutionOptions.Default, cancellationToken);
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var commandBus = commandRouter.FindRoute(command.GetType())
                             ?? throw new ArgumentException($"No route to a command bus found for command type {command.GetType()}");
            return commandBus.SendAsync(command, executionOptions, cancellationToken);
        }

        public Task SendAsync(ICommandBase command, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(command, CommandExecutionOptions.Default, cancellationToken);
        }
    }
}