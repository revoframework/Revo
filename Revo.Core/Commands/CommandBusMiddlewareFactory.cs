using System.Linq;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    public class CommandBusMiddlewareFactory : ICommandBusMiddlewareFactory
    {
        private readonly IServiceLocator serviceLocator;

        public CommandBusMiddlewareFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public ICommandBusMiddleware<TCommand>[] CreateMiddlewares<TCommand>(ICommandBus commandBus)
            where TCommand : class, ICommandBase
        {
            return serviceLocator.GetAll<ICommandBusMiddleware<TCommand>>()
                .ToArray();
        }
    }
}