using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    /// <summary>
    /// Decorates command handlers with an execution pipeline implemented by handler middlewares
    /// and filters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandBusPipeline : ICommandBusPipeline
    {
        private readonly ICommandBusMiddlewareFactory middlewareFactory;

        public CommandBusPipeline(ICommandBusMiddlewareFactory middlewareFactory)
        {
            this.middlewareFactory = middlewareFactory;
        }
        
        public Task<object> ProcessAsync(ICommandBase command, CommandBusMiddlewareDelegate executionHandler,
            ICommandBus commandBus, CommandExecutionOptions executionOptions, CancellationToken cancellationToken)
        {
            var processMethod = GetType().GetRuntimeMethods().Single(x => x.Name == nameof(ProcessInternalAsync));
            var boundProcessMethod = processMethod.MakeGenericMethod(command.GetType());

            return (Task<object>)boundProcessMethod.Invoke(this, new object[]
            {
                command, executionHandler, commandBus, executionOptions, cancellationToken
            });
        }

        private async Task<object> ProcessInternalAsync<T>(T command, CommandBusMiddlewareDelegate executionHandler,
            ICommandBus commandBus, CommandExecutionOptions executionOptions, CancellationToken cancellationToken)
            where T : class, ICommandBase
        {
            using (TaskContext.Enter())
            {
                var middlewares = middlewareFactory.CreateMiddlewares<T>(commandBus);

                if (middlewares.Length == 0)
                {
                    return executionHandler(command);
                }

                middlewares = middlewares.OrderBy(x => x.Order).ToArray();

                return await ProcessNextMiddleware(command, middlewares, 0, executionHandler,
                    executionOptions, cancellationToken);
            }
        }

        private async Task<object> ProcessNextMiddleware<T>(T command, ICommandBusMiddleware<T>[] middlewares,
            int middlewareIndex, CommandBusMiddlewareDelegate executionHandler,
            CommandExecutionOptions executionOptions, CancellationToken cancellationToken)
            where T : class, ICommandBase
        {
            CommandBusMiddlewareDelegate next;
            if (middlewareIndex == middlewares.Length - 1)
            {
                next = executionHandler;
            }
            else
            {
                next = async processedCommand =>
                {
                    var typedCommand = processedCommand as T
                                       ?? throw new ArgumentException(
                                           $"Command passed to command bus middleware ({processedCommand?.GetType()}) is not of original type {typeof(T)}");
                    return await ProcessNextMiddleware<T>(typedCommand, middlewares,
                        middlewareIndex + 1, executionHandler, executionOptions, cancellationToken);
                };
            }

            return await middlewares[middlewareIndex].HandleAsync(command, executionOptions, next, cancellationToken);
        }
    }
}
