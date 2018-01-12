using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Core.Transactions
{
    public interface IUnitOfWork : ITransaction
    {
        IPublishEventBuffer EventBuffer { get; }
    }
}
