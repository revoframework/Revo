using System;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Transactions;

namespace Revo.Core.Commands
{
    public class UnitOfWorkCommandFilter : IPreCommandFilter<ICommandBase>,
        IPostCommandFilter<ICommandBase>, IExceptionCommandFilter<ICommandBase>
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly CommandContextStack commandContextStack;

        public UnitOfWorkCommandFilter(IUnitOfWorkFactory unitOfWorkFactory,
            CommandContextStack commandContextStack)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.commandContextStack = commandContextStack;
        }

        public Task PreFilterAsync(ICommandBase command)
        {
            ICommandContext newContext = new CommandContext(command, unitOfWorkFactory.CreateUnitOfWork());
            commandContextStack.Push(newContext);

            return Task.FromResult(0);
        }
        
        public async Task PostFilterAsync(ICommandBase command, object result)
        {
            try
            {
                if (commandContextStack.UnitOfWork != null
                    && !command.GetType().GetInterfaces().Any(
                        x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IQuery<>)))
                {
                    await commandContextStack.UnitOfWork.CommitAsync();
                }
            }
            finally
            {
                if (commandContextStack.PeekOrDefault != null)
                {
                    commandContextStack.Pop();
                }
            }
        }

        public Task FilterExceptionAsync(ICommandBase command, Exception e)
        {
            if (commandContextStack.PeekOrDefault != null)
            {
                // commandContextStack.UnitOfWork.Dispose(); // TODO maybe?
                commandContextStack.Pop();
            }

            return Task.FromResult(0);
        }
    }
}
