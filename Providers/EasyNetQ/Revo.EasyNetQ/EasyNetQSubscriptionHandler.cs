using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQSubscriptionHandler : IEasyNetQSubscriptionHandler
    {
        private readonly IEventBus eventBus;

        public EasyNetQSubscriptionHandler(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public async Task HandleMessage(object message)
        {
            if (message is IEventMessage eventMessage)
            {
                using (TaskContext.Enter())
                {
                    await eventBus.PublishAsync(eventMessage);
                }
            }
        }
    }
}
