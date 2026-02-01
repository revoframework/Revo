using Revo.Core.Events;
using System;

namespace Revo.Core.Transactions
{
    public class UnitOfWorkFactory(Func<Lazy<IUnitOfWorkListener[]>> unitOfWorkListenersLazyFunc, IPublishEventBufferFactory publishEventBufferFactory) : IUnitOfWorkFactory
    {
        private readonly Func<Lazy<IUnitOfWorkListener[]>> unitOfWorkListenersLazyFunc = unitOfWorkListenersLazyFunc;
        private readonly IPublishEventBufferFactory publishEventBufferFactory = publishEventBufferFactory;

        public IUnitOfWork CreateUnitOfWork()
        {
            var tx = new UnitOfWork(unitOfWorkListenersLazyFunc(), publishEventBufferFactory.CreateEventBuffer());

            return tx;
        }
    }
}
