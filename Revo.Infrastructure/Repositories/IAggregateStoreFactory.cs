using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    public interface IAggregateStoreFactory
    {
        bool CanHandleAggregateType(Type aggregateType);
        IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork);
    }
}
