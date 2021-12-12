using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Projections;
using Revo.Infrastructure.Tenancy;
using System;
using System.Collections.Generic;

namespace Revo.EF6.Projections
{
    public class EF6ProjectionEventListener : ProjectionEventListener
    {
        public EF6ProjectionEventListener(Func<IEF6ProjectionSubSystem> projectionSubSystemFunc,
            IUnitOfWorkFactory unitOfWorkFactory, Func<CommandContextStack> commandContextStackFunc,
            ITenantProvider tenantProvider, EF6ProjectionEventSequencer eventSequencer) :
            base(projectionSubSystemFunc, unitOfWorkFactory, commandContextStackFunc, tenantProvider)
        {
            EventSequencer = eventSequencer;
        }

        public override IAsyncEventSequencer EventSequencer { get; }
        
        public class EF6ProjectionEventSequencer : AsyncEventSequencer<DomainAggregateEvent>
        {
            public readonly string QueueNamePrefix = "EF6ProjectionEventListener:";

            protected override IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<DomainAggregateEvent> message)
            {
                yield return new EventSequencing()
                {
                    SequenceName = QueueNamePrefix + message.Event.AggregateId.ToString(),
                    EventSequenceNumber = message.Metadata.GetStreamSequenceNumber()
                };
            }
            
            protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<DomainAggregateEvent> message)
            {
                return true;
            }
        }
    }
}
