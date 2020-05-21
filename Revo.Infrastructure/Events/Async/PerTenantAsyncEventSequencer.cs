using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Async
{
    public class PerTenantAsyncEventSequencer : IAsyncEventSequencer
    {
        protected PerTenantAsyncEventSequencer(string queueNamePrefix)
        {
            QueueNamePrefix = queueNamePrefix;
        }

        public string QueueNamePrefix { get; }

        public IEnumerable<EventSequencing> GetEventSequencing(IEventMessage message)
        {
            yield return new EventSequencing()
            {
                SequenceName = QueueNamePrefix + ":" + (message.Metadata.GetAggregateTenantId()?.ToString() ?? "null"),
                EventSequenceNumber = null
            };
        }

        public virtual bool ShouldAttemptSynchronousDispatch(IEventMessage message)
        {
            return false;
        }
    }
}