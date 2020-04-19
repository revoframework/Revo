using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    public class LocalCommandBus : ILocalCommandBus
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ICommandBusPipeline pipeline;

        public LocalCommandBus(IServiceLocator serviceLocator, ICommandBusPipeline pipeline)
        {
            this.serviceLocator = serviceLocator;
            this.pipeline = pipeline;
        }

        public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken)
        {
            Type handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            var result = await InvokeHandleAsync<TResult>(handlerType, command, executionOptions, cancellationToken);
            return result;
        }

        public Task SendAsync(ICommandBase command, CancellationToken cancellationToken)
        {
            return SendAsync(command, CommandExecutionOptions.Default, cancellationToken);
        }

        public Task SendAsync(ICommandBase command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken)
        {
            Type genericCommandType = command.GetType().GetInterfaces().FirstOrDefault(
                x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(ICommand<>));
            Type handlerType;

            if (genericCommandType != null)
            {
                handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(),
                    genericCommandType.GetGenericArguments()[0]);
                return InvokeHandleAsync<object>(handlerType, command, executionOptions, cancellationToken);
            }

            handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            return InvokeHandleAsync<object>(handlerType, command, executionOptions, cancellationToken);
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
        {
            return SendAsync<TResult>(command, CommandExecutionOptions.Default, cancellationToken);
        }

        private async Task<TResult> InvokeHandleAsync<TResult>(Type handlerType, ICommandBase command, CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken)
        {
            CommandBusMiddlewareDelegate executionHandler = async processedCommand =>
            {
                if (processedCommand == null)
                {
                    throw new ArgumentNullException(nameof(processedCommand));
                }

                if (!command.GetType().IsInstanceOfType(processedCommand))
                {
                    throw new ArgumentException(
                        $"Command passed back to command bus from a middleware is not of its original type {command.GetType()}");
                }

                Type commandType = command.GetType();
                object listener = serviceLocator.Get(handlerType);
                var handleMethod = listener.GetType()
                    .GetRuntimeMethod(nameof(ICommandHandler<ICommand>.HandleAsync),
                        new[] { commandType, typeof(CancellationToken) });

                var resultTask = (Task) handleMethod.Invoke(listener, new object[] {command, cancellationToken});

                if (resultTask is Task<TResult> resultObjectTask)
                {
                    return await resultObjectTask;
                }
                else
                {
                    await resultTask;
                    return null;
                }
            };

            object result = await pipeline.ProcessAsync(command, executionHandler, this,
                executionOptions, cancellationToken);
            return (TResult) result;
            // TODO test with contravariant handlers
        }
    }
}
