using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Projections;

namespace Revo.RavenDB.Projections
{
    public class RavenProjectionEventListener : ProjectionEventListener
    {
        public RavenProjectionEventListener(IRavenProjectionSubSystem projectionSubSystem,
            IUnitOfWorkFactory unitOfWorkFactory, CommandContextStack commandContextStack,
            RavenProjectionEventSequencer eventSequencer) :
            base(projectionSubSystem, unitOfWorkFactory, commandContextStack)
        {
            EventSequencer = eventSequencer;
        }
        
        public override IAsyncEventSequencer EventSequencer { get; }

        public class RavenProjectionEventSequencer : AsyncEventSequencer<DomainAggregateEvent>
        {
            public readonly string QueueNamePrefix = "RavenProjectionEventSequencer:";

            protected override IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<DomainAggregateEvent> message)
            {
                yield return new EventSequencing() { SequenceName = QueueNamePrefix + message.Event.AggregateId.ToString(),
                    EventSequenceNumber = message.Metadata.GetStreamSequenceNumber() };
            }
            
            protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<DomainAggregateEvent> message)
            {
                return true;
            }
        }
    }
}
