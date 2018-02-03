using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Events
{
    public class PublishEventBuffer : IPublishEventBuffer
    {
        private readonly IEventBus eventBus;
        private readonly List<IEventMessage> messages = new List<IEventMessage>();

        public PublishEventBuffer(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public IReadOnlyCollection<IEventMessage> Events => messages;
        
        public async Task FlushAsync(CancellationToken cancellationToken)
        {
            foreach (IEventMessage message in messages) // TODO exception handling (i.e. throw away the remaining events as well?)
            {
                await eventBus.PublishAsync(message, cancellationToken);
            }

            messages.Clear();
        }

        public void PushEvent(IEventMessage message)
        {
            messages.Add(message);
        }
    }
}
