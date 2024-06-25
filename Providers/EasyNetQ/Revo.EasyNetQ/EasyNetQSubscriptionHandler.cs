using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQSubscriptionHandler(IEventBus eventBus) : IEasyNetQSubscriptionHandler
    {
        public async Task HandleMessageAsync(object message)
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
