using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.Projections;

namespace GTRevo.Infrastructure.EF6.Projections
{
    public class EF6ProjectionEventListener : ProjectionEventListener
    {
        private readonly IServiceLocator serviceLocator;

        public EF6ProjectionEventListener(IEventSourcedAggregateRepository eventSourcedRepository,
            IEntityTypeManager entityTypeManager, IServiceLocator serviceLocator,
            EF6ProjectionEventSequencer eventSequencer) :
            base(eventSourcedRepository, entityTypeManager)
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
