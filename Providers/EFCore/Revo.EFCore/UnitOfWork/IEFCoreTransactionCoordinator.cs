using System;
using System.Collections.Generic;
using System.Text;
using Revo.Core.Transactions;

namespace Revo.EFCore.UnitOfWork
{
    public interface IEFCoreTransactionCoordinator : ITransactionCoordinator, ITransaction
    {
    }
}
