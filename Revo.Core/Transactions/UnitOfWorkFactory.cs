using Revo.Core.Events;
using System;

namespace Revo.Core.Transactions
{
    public class UnitOfWorkFactory(Func<Lazy<IUnitOfWorkListener[]>> unitOfWorkListenersLazyFunc,
        IPublishEventBufferFactory publishEventBufferFactory) : IUnitOfWorkFactory
    {
        public IUnitOfWork CreateUnitOfWork()
        {
            var tx = new UnitOfWork(unitOfWorkListenersLazyFunc(), publishEventBufferFactory.CreateEventBuffer());
            return tx;
        }
    }
}
