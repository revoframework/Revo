using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Transactions;

namespace Revo.Core.Commands
{
    public class UnitOfWorkCommandBusMiddleware : ICommandBusMiddleware<ICommandBase>
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly CommandContextStack commandContextStack;

        public UnitOfWorkCommandBusMiddleware(IUnitOfWorkFactory unitOfWorkFactory,
            CommandContextStack commandContextStack)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.commandContextStack = commandContextStack;
        }

        public int Order { get; set; } = -2000;

        public Task<object> HandleAsync(ICommandBase command, CommandExecutionOptions executionOptions,
            CommandBusMiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if (executionOptions.AutoCommitUnitOfWork == true
                || (executionOptions.AutoCommitUnitOfWork == null && !IsQuery(command)))
            {
                using (var uow = unitOfWorkFactory.CreateUnitOfWork())
                {
                    return HandleNext(command, next, uow);
                }
            }
            else
            {
                return HandleNext(command, next, null);
            }
        }

        private async Task<object> HandleNext(ICommandBase command, CommandBusMiddlewareDelegate next,
            IUnitOfWork unitOfWork)
        {
            ICommandContext newContext = new CommandContext(command, unitOfWork);
            commandContextStack.Push(newContext);

            newContext.UnitOfWork?.Begin();
            try
            {
                var result = await next(command);

                if (newContext.UnitOfWork != null)
                {
                    await newContext.UnitOfWork.CommitAsync();
                }

                return result;
            }
            finally
            {
                Debug.Assert(commandContextStack.PeekOrDefault == newContext);
                commandContextStack.Pop();
            }
        }

        private bool IsQuery(ICommandBase command)
        {
            return command.GetType().GetInterfaces().Any(
                x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IQuery<>));
        }
    }
}
