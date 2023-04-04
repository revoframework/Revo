using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Events;

public class EventBus(IServiceProvider serviceProvider) : IEventBus
{
    public async Task PublishAsync(IEventMessage message, CancellationToken cancellationToken)
    {
        Type messageType = typeof(IEventMessage<>).MakeGenericType(message.Event.GetType());
        Type listenerType = typeof(IEventListener<>).MakeGenericType(message.Event.GetType());
        var handleMethod = listenerType.GetMethod("HandleAsync", new[] { messageType, typeof(CancellationToken)});
        IEnumerable<object> listeners = serviceProvider.GetServices(listenerType);

        foreach (var listener in listeners)
        {
            await (Task) handleMethod.Invoke(listener, new object[] {message, cancellationToken});
        }
    }
}