using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;

namespace GTRevo.Core.Commands
{
    public class CommandBus : ICommandBus
    {
        private readonly IServiceLocator serviceLocator;

        public CommandBus(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
        {
            Type handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            return (Task<TResult>) InvokeHandleAsync(handlerType, command, cancellationToken);
        }

        public Task SendAsync(ICommandBase command, CancellationToken cancellationToken)
        {
            Type genericCommandType = command.GetType().GetInterfaces().FirstOrDefault(
                x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(ICommand<>));
            Type handlerType;

            if (genericCommandType != null)
            {
                handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(),
                    genericCommandType.GetGenericArguments()[0]);
                return InvokeHandleAsync(handlerType, command, cancellationToken);
            }

            handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            return InvokeHandleAsync(handlerType, command, cancellationToken);
        }

        private Task InvokeHandleAsync(Type handlerType, ICommandBase command, CancellationToken cancellationToken)
        {
            Type commandType = command.GetType();
            object listener = serviceLocator.Get(handlerType);
            var handleMethod = listener.GetType()
                .GetRuntimeMethod(nameof(ICommandHandler<ICommand>.HandleAsync), new[] { commandType, typeof(CancellationToken) });
            return (Task)handleMethod.Invoke(listener, new object[] { command, cancellationToken });
            // TODO test with contravariant handlers
        }
    }
}
