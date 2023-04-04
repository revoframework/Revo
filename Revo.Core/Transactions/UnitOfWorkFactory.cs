using Revo.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Services;

namespace Revo.Core.Transactions;

public class UnitOfWorkFactory(
    IServiceFactory<IUnitOfWorkListener> unitOfWorkListenersLazyFunc,
    IPublishEventBufferFactory publishEventBufferFactory)
    : IUnitOfWorkFactory
{
    public IUnitOfWork CreateUnitOfWork()
    {
        var unitOfWorkListeners = new Lazy<IEnumerable<IUnitOfWorkListener>>(unitOfWorkListenersLazyFunc.GetServices);
        var tx = new UnitOfWork(unitOfWorkListeners, publishEventBufferFactory.CreateEventBuffer());
        return tx;
    }
}