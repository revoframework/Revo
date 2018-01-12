using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;

namespace GTRevo.Core.Commands
{
    public class UnitOfWorkCommandFilter : IPreCommandFilter<ICommandBase>,
        IPostCommandFilter<ICommandBase>, IExceptionCommandFilter<ICommandBase>
    {
        private readonly IUnitOfWork unitOfWork;

        public UnitOfWorkCommandFilter(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public Task PreFilterAsync(ICommandBase command)
        {
            return Task.FromResult(0);
        }
        
        public async Task PostFilterAsync(ICommandBase command, object result)
        {
            if (unitOfWork != null
                && !command.GetType().GetInterfaces().Any(
                    x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IQuery<>)))
            {
                try
                {
                    await unitOfWork.CommitAsync();
                }
                finally
                {
                }
            }
        }

        public Task FilterExceptionAsync(ICommandBase command, Exception e)
        {
            if (unitOfWork != null)
            {
                unitOfWork.Dispose(); // TODO: maybe?
            }

            return Task.FromResult(0);
        }
    }
}
