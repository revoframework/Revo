using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreProjectionEventListener : ProjectionEventListener
    {
        public EFCoreProjectionEventListener(IEFCoreProjectionSubSystem projectionSubSystem,
            IUnitOfWorkFactory unitOfWorkFactory, CommandContextStack commandContextStack,
            EFCoreProjectionEventSequencer eventSequencer) :
            base(projectionSubSystem, unitOfWorkFactory, commandContextStack)
        {
            EventSequencer = eventSequencer;
        }
        
        public override IAsyncEventSequencer EventSequencer { get; }
        
        public class EFCoreProjectionEventSequencer : AsyncEventSequencer<DomainAggregateEvent>
        {
            public readonly string QueueNamePrefix = "EFCoreProjectionEventListener:";

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
