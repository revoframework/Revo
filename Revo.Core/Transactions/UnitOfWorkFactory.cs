using System;
using System.Linq;
using Revo.Core.Events;

namespace Revo.Core.Transactions
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IUnitOfWorkListener[] unitOfWorkListeners;
        private readonly IPublishEventBufferFactory publishEventBufferFactory;

        public UnitOfWorkFactory(
            IUnitOfWorkListener[] unitOfWorkListeners,
            IPublishEventBufferFactory publishEventBufferFactory)
        {
            this.unitOfWorkListeners = unitOfWorkListeners;
            this.publishEventBufferFactory = publishEventBufferFactory;
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            var tx = new UnitOfWork(unitOfWorkListeners, publishEventBufferFactory.CreatEventBuffer());
            return tx;
        }
    }
}
