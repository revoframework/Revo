using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    internal class CrudAggregateStoreFactory : IAggregateStoreFactory
    {
        private readonly Func<IPublishEventBuffer, CrudAggregateStore> func;

        public CrudAggregateStoreFactory(Func<IPublishEventBuffer, CrudAggregateStore> func)
        {
            this.func = func;
        }

        public IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return func(unitOfWork.EventBuffer);
        }
    }
}
