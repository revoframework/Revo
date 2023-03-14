using System;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    public interface IAggregateStoreFactory
    {
        bool CanHandleAggregateType(Type aggregateType);
        IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork);
    }
}
