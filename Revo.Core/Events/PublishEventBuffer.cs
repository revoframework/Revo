using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Events
{
    public class PublishEventBuffer(IEventBus eventBus) : IPublishEventBuffer
    {
        private readonly List<IEventMessage> messages = new List<IEventMessage>();

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
