using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EventTransport<TEvent, TPublishAsEvent> : IEventListener<TEvent>
        where TEvent : TPublishAsEvent
        where TPublishAsEvent : IEvent
    {
        private readonly IEasyNetQBus easyNetQBus;

        public EventTransport(IEasyNetQBus easyNetQBus)
        {
            this.easyNetQBus = easyNetQBus;
        }

        public async Task HandleAsync(IEventMessage<TEvent> message, CancellationToken cancellationToken)
        {
            await easyNetQBus.PublishAsync((IEventMessage<TPublishAsEvent>)message);
        }
    }
}
