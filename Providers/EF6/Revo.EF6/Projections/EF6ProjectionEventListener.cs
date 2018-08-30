using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Projections;

namespace Revo.EF6.Projections
{
    public class EF6ProjectionEventListener : ProjectionEventListener
    {
        private readonly IServiceLocator serviceLocator;

        public EF6ProjectionEventListener(IEntityTypeManager entityTypeManager,
            IServiceLocator serviceLocator, EF6ProjectionEventSequencer eventSequencer) :
            base(entityTypeManager)
        {
            this.serviceLocator = serviceLocator;
            EventSequencer = eventSequencer;
        }
        
        public override IAsyncEventSequencer EventSequencer { get; }

        public override IEnumerable<IEntityEventProjector> GetProjectors(Type entityType)
        {
            return serviceLocator.GetAll(
                    typeof(IEF6EntityEventProjector<>).MakeGenericType(entityType))
                .Cast<IEntityEventProjector>();
        }

        public class EF6ProjectionEventSequencer : AsyncEventSequencer<DomainAggregateEvent>
        {
            public readonly string QueueNamePrefix = "EF6ProjectionEventListener:";

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
