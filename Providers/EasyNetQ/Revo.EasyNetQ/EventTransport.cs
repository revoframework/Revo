using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EventTransport<TEvent, TPublishAsEvent>(IEasyNetQBus easyNetQBus) : IEventListener<TEvent>
        where TEvent : TPublishAsEvent
        where TPublishAsEvent : IEvent
    {
        public async Task HandleAsync(IEventMessage<TEvent> message, CancellationToken cancellationToken)
        {
            await easyNetQBus.PublishAsync((IEventMessage<TPublishAsEvent>)message);
        }
    }
}
