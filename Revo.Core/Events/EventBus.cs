using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Core.Events
{
    public class EventBus : IEventBus
    {
        private readonly IServiceLocator serviceLocator;

        public EventBus(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public async Task PublishAsync(IEventMessage message, CancellationToken cancellationToken)
        {
            Type messageType = typeof(IEventMessage<>).MakeGenericType(message.Event.GetType());
            Type listenerType = typeof(IEventListener<>).MakeGenericType(message.Event.GetType());
            var handleMethod = listenerType.GetMethod("HandleAsync", new[] { messageType, typeof(CancellationToken)});
            IEnumerable<object> listeners = serviceLocator.GetAll(listenerType);

            foreach (var listener in listeners)
            {
                await (Task) handleMethod.Invoke(listener, new object[] {message, cancellationToken});
            }
        }
    }
}
