using Revo.Core.Events;
using System;

namespace Revo.Core.Transactions
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly Func<Lazy<IUnitOfWorkListener[]>> unitOfWorkListenersLazyFunc;
        private readonly IPublishEventBufferFactory publishEventBufferFactory;

        public UnitOfWorkFactory(Func<Lazy<IUnitOfWorkListener[]>> unitOfWorkListenersLazyFunc, IPublishEventBufferFactory publishEventBufferFactory)
        {
            this.unitOfWorkListenersLazyFunc = unitOfWorkListenersLazyFunc;
            this.publishEventBufferFactory = publishEventBufferFactory;
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            var tx = new UnitOfWork(unitOfWorkListenersLazyFunc(), publishEventBufferFactory.CreateEventBuffer());
            return tx;
        }
    }
}
