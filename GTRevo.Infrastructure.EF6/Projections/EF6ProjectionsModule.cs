using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Events.Async;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EF6.Projections
{
    public class EF6ProjectionsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventSequencer<DomainAggregateEvent>, EF6ProjectionEventListener.EF6ProjectionEventSequencer>()
                .To<EF6ProjectionEventListener.EF6ProjectionEventSequencer>()
                .InRequestOrJobScope();

            Bind<IAsyncEventListener<DomainAggregateEvent>>()
                .To<EF6ProjectionEventListener>()
                .InRequestOrJobScope();
        }
    }
}
