using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQBlockingSubscriptionHandler(IServiceScopeFactory serviceScopeFactory)
        : IEasyNetQBlockingSubscriptionHandler
    {
        public void HandleMessage(object message)
        {
            if (message is IEventMessage eventMessage)
            {
                Task.Run(async () =>
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                    await eventBus.PublishAsync(eventMessage);
                }).GetAwaiter().GetResult();
            }
        }
    }
}