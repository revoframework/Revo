using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQBlockingSubscriptionHandler(IEventBus eventBus) : IEasyNetQBlockingSubscriptionHandler
    {
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