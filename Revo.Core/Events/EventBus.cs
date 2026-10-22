using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Services;

namespace Revo.Core.Events;

public class EventBus(IVariantServiceResolver serviceResolver) : IEventBus
{
    public async Task PublishAsync(IEventMessage message, CancellationToken cancellationToken)
    {
        Type messageType = typeof(IEventMessage<>).MakeGenericType(message.Event.GetType());
        Type listenerType = typeof(IEventListener<>).MakeGenericType(message.Event.GetType());
        var handleMethod = listenerType.GetMethod("HandleAsync", new[] { messageType, typeof(CancellationToken)});
        // variant resolution, so that e.g. IEventListener<IEvent> receives all events
        IEnumerable<object> listeners = serviceResolver.GetServices(listenerType);

        foreach (var listener in listeners)
        {
            await (Task) handleMethod.Invoke(listener, new object[] {message, cancellationToken});
        }
    }
}