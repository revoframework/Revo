using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQSubscriptionHandler(IServiceScopeFactory serviceScopeFactory)
        : IEasyNetQSubscriptionHandler
    {
        public async Task HandleMessageAsync(object message)
        {
            if (message is IEventMessage eventMessage)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                await eventBus.PublishAsync(eventMessage);
            }
        }
    }
}
