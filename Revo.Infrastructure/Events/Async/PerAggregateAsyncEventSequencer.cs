using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Async
{
    public class PerAggregateAsyncEventSequencer : IAsyncEventSequencer
    {
        public PerAggregateAsyncEventSequencer(string queueNamePrefix)
        {
            QueueNamePrefix = queueNamePrefix;
        }

        public string QueueNamePrefix { get; }

        public IEnumerable<EventSequencing> GetEventSequencing(IEventMessage message)
        {
            yield return new EventSequencing()
            {
                SequenceName = QueueNamePrefix + ":" + (message.Event as DomainAggregateEvent)?.AggregateId.ToString(),
                EventSequenceNumber = message.Metadata.GetStreamSequenceNumber()
            };
        }

        public virtual bool ShouldAttemptSynchronousDispatch(IEventMessage message)
        {
            return false;
        }
    }
}