using System.Linq;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    public class CommandBusMiddlewareFactory(IServiceLocator serviceLocator) : ICommandBusMiddlewareFactory
    {
        public ICommandBusMiddleware<TCommand>[] CreateMiddlewares<TCommand>(ICommandBus commandBus)
            where TCommand : class, ICommandBase
        {
            return serviceLocator.GetAll<ICommandBusMiddleware<TCommand>>()
                .ToArray();
        }
    }
}