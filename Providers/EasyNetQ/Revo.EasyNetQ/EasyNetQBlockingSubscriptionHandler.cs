using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQBlockingSubscriptionHandler : IEasyNetQBlockingSubscriptionHandler
    {
        private readonly IEventBus eventBus;

        public EasyNetQBlockingSubscriptionHandler(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void HandleMessage(object message)
        {
            if (message is IEventMessage eventMessage)
            {
                Task.Factory.StartNewWithContext(async () =>
                {
                    await eventBus.PublishAsync(eventMessage);
                }).GetAwaiter().GetResult();
            }
        }
    }
}