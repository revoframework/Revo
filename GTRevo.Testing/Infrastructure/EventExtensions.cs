using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Events;

namespace GTRevo.Testing.Infrastructure
{
    public static class EventExtensions
    {
        public static IEventMessageDraft<TEvent> ToMessageDraft<TEvent>(this TEvent @event)
            where TEvent : class, IEvent
        {
            Type eventType = @event.GetType();
            Type messageType = typeof(EventMessageDraft<>).MakeGenericType(eventType);
            return (IEventMessageDraft<TEvent>) messageType.GetConstructor(new[] {eventType}).Invoke(new[] {@event});
        }
    }
}
