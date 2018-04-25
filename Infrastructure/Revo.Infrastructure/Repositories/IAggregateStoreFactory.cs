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
        IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork);
    }
}
